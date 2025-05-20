using System.Text.Json;
using MessagePack;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NLightning.Application.NLTG.Services;

using Common.Options;
using Domain.Bitcoin.Services;
using Domain.Money;
using Models;

public class FeeService : IFeeService
{
    private const string FEE_CACHE_FILE_NAME = "fee_cache.bin";
    private static readonly TimeSpan s_defaultCacheExpiration = TimeSpan.FromMinutes(5);

    private DateTime _lastFetchTime = DateTime.MinValue;
    private LightningMoney _cachedFeeRate = LightningMoney.Zero;
    private Task? _feeTask;
    private CancellationTokenSource? _cts;

    private readonly HttpClient _httpClient;
    private readonly ILogger<FeeService> _logger;
    private readonly TimeSpan _cacheTimeExpiration;
    private readonly string _cacheFilePath;
    private readonly FeeEstimationOptions _feeEstimationOptions;

    public FeeService(IOptions<FeeEstimationOptions> feeOptions, HttpClient httpClient, ILogger<FeeService> logger)
    {
        _feeEstimationOptions = feeOptions.Value;
        _httpClient = httpClient;
        _logger = logger;

        _cacheFilePath = ParseFilePath(_feeEstimationOptions);
        _cacheTimeExpiration = ParseCacheTime(_feeEstimationOptions.CacheExpiration);

        // Try to load from the file initially
        _ = LoadFromFileAsync(CancellationToken.None);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Start the background task
        _feeTask = RunPeriodicRefreshAsync(_cts.Token);

        // If the cache from the file is not valid, refresh immediately
        if (!IsCacheValid())
        {
            await RefreshFeeRateAsync(_cts.Token);
        }
    }

    public async Task StopAsync()
    {
        if (_cts is null)
        {
            throw new InvalidOperationException("Service is not running");
        }

        await _cts.CancelAsync();

        if (_feeTask is not null)
        {
            try
            {
                await _feeTask;
            }
            catch (OperationCanceledException)
            {
                // Expected during cancellation
            }
        }
    }

    public async Task<LightningMoney> GetFeeRatePerKwAsync(CancellationToken cancellationToken = default)
    {
        if (IsCacheValid())
        {
            return _cachedFeeRate;
        }

        using var linkedCts = CancellationTokenSource
            .CreateLinkedTokenSource(cancellationToken, _cts?.Token ?? CancellationToken.None);

        await RefreshFeeRateAsync(linkedCts.Token);
        return _cachedFeeRate;
    }

    public LightningMoney GetCachedFeeRatePerKw()
    {
        return _cachedFeeRate;
    }

    public async Task RefreshFeeRateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var feeRate = await FetchFeeRateFromApiAsync(cancellationToken);
            _cachedFeeRate.Satoshi = feeRate;
            _lastFetchTime = DateTime.UtcNow;
            await SaveToFileAsync();
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error fetching fee rate from API");
        }
    }

    private async Task<long> FetchFeeRateFromApiAsync(CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        try
        {
            if (_feeEstimationOptions.Method.Equals("GET", StringComparison.CurrentCultureIgnoreCase))
            {
                response = await _httpClient.GetAsync(_feeEstimationOptions.Url, cancellationToken);
            }
            else // POST
            {
                var content = new StringContent(
                    _feeEstimationOptions.Body,
                    System.Text.Encoding.UTF8,
                    _feeEstimationOptions.ContentType);

                response = await _httpClient.PostAsync(_feeEstimationOptions.Url, content, cancellationToken);
            }
        }
        catch (Exception e)
        {
            throw new InvalidOperationException("Error fetching from API", e);
        }

        response.EnsureSuccessStatusCode();
        var jsonResponseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Parse the JSON response
        using var document =
            await JsonDocument.ParseAsync(jsonResponseStream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        // Extract the preferred fee rate from the JSON response
        if (!root.TryGetProperty(_feeEstimationOptions.PreferredFeeRate, out var feeRateElement))
        {
            throw new InvalidOperationException(
                $"Could not extract {_feeEstimationOptions.PreferredFeeRate} from API response.");
        }

        // Parse the fee rate value
        if (!feeRateElement.TryGetDecimal(out var feeRate))
        {
            throw new InvalidOperationException(
                $"Could not extract {_feeEstimationOptions.PreferredFeeRate} from API response.");
        }

        // Apply the multiplier to convert to sat/kw
        if (decimal.TryParse(_feeEstimationOptions.RateMultiplier, out var multiplier))
        {
            return (long)(feeRate * multiplier);
        }

        throw new InvalidOperationException(
            $"Could not extract {_feeEstimationOptions.PreferredFeeRate} from API response.");
    }

    private async Task RunPeriodicRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Refresh if it's not canceled
                if (!cancellationToken.IsCancellationRequested)
                {
                    await RefreshFeeRateAsync(cancellationToken);

                    // Wait for the cache time or until cancellation
                    await Task.Delay(_cacheTimeExpiration, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Stopping fee service");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in fee service");
        }
    }

    private async Task SaveToFileAsync()
    {
        _logger.LogDebug("Saving fee rate to file {filePath}", _cacheFilePath);

        try
        {
            var cacheData = new FeeRateCacheData
            {
                FeeRate = _cachedFeeRate,
                LastFetchTime = _lastFetchTime
            };

            await using var fileStream = File.OpenWrite(_cacheFilePath);
            await MessagePackSerializer.SerializeAsync(fileStream, cacheData, cancellationToken: CancellationToken.None);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving fee rate to file");
        }
    }

    private async Task LoadFromFileAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading fee rate from file {filePath}", _cacheFilePath);

        try
        {
            if (!File.Exists(_cacheFilePath))
            {
                _logger.LogDebug("Fee rate cache file does not exist. Skipping load.");
                return;
            }

            await using var fileStream = File.OpenRead(_cacheFilePath);
            var cacheData =
                await MessagePackSerializer.DeserializeAsync<FeeRateCacheData?>(fileStream,
                    cancellationToken: cancellationToken);

            if (cacheData == null)
            {
                _logger.LogDebug("Fee rate cache file is empty. Skipping load.");
                return;
            }

            _cachedFeeRate = cacheData.FeeRate;
            _lastFetchTime = cacheData.LastFetchTime;
        }
        catch (OperationCanceledException)
        {
            // Ignore cancellation
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading fee rate from file");
        }
    }

    private bool IsCacheValid()
    {
        return !_cachedFeeRate.IsZero && DateTime.UtcNow.Subtract(_lastFetchTime).CompareTo(_cacheTimeExpiration) <= 0;
    }

    private static TimeSpan ParseCacheTime(string cacheTime)
    {
        try
        {
            // Parse formats like "5m", "1hour", "30s"
            var valueStr = new string(cacheTime.Where(char.IsDigit).ToArray());
            var unit = new string(cacheTime.Where(char.IsLetter).ToArray()).ToLowerInvariant();

            if (!int.TryParse(valueStr, out var value))
                return s_defaultCacheExpiration;

            return unit switch
            {
                "s" or "second" or "seconds" => TimeSpan.FromSeconds(value),
                "m" or "minute" or "minutes" => TimeSpan.FromMinutes(value),
                "h" or "hour" or "hours" => TimeSpan.FromHours(value),
                "d" or "day" or "days" => TimeSpan.FromDays(value),
                _ => TimeSpan.FromMinutes(5)
            };
        }
        catch
        {
            return s_defaultCacheExpiration; // Default on error
        }
    }

    private static string ParseFilePath(FeeEstimationOptions feeEstimationOptions)
    {
        var filePath = feeEstimationOptions.CacheFile;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, FEE_CACHE_FILE_NAME);
        }

        // Check if the file path is absolute or relative
        return Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(Directory.GetCurrentDirectory(), filePath); // If it's relative, combine it with the current directory
    }
}