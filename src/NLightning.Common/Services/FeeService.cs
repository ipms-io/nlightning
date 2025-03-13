using System.Text.Json;
using MessagePack;

namespace NLightning.Common.Services;

using Exceptions;
using Interfaces;
using Managers;
using Types;

public class FeeService : IFeeService, IDisposable
{
    private const string FEE_CACHE_FILE_NAME = "fee_cache.bin";
    private static readonly TimeSpan s_defaultCacheExpiration = TimeSpan.FromMinutes(5);

    private static DateTime s_lastFetchTime = DateTime.MinValue;
    private static ulong s_cachedFeeRate;
    private static Task? s_backgroundTask;
    private static CancellationTokenSource? s_cts;

    private readonly HttpClient _httpClient;
    private readonly TimeSpan _cacheTimeExpiration;
    private readonly string _cacheFilePath;

    public FeeService(HttpClient httpClient)
    {
        // Never allow file to exist more than once
        if (s_backgroundTask is not null)
        {
            throw new WarningException("This class should behave like a Singleton. Please, initialize it only once.");
        }

        _httpClient = httpClient;
        _cacheFilePath = ParseFilePath();
        _cacheTimeExpiration = ParseCacheTime(ConfigManager.Instance.FeeEstimationCacheExpiration);

        // Try to load from file initially
        _ = LoadFromFileAsync(default);
    }

    public async Task StartBackgroundRefreshAsync(CancellationToken cancellationToken = default)
    {
        if (s_backgroundTask != null)
        {
            throw new WarningException("You should not be calling this method multiple times.");
        }

        try
        {
            if (s_backgroundTask != null)
            {
                throw new WarningException("You should not be calling this method multiple times.");
            }

            s_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start the background task
            s_backgroundTask = RunPeriodicRefreshAsync(s_cts.Token);

            // If cache from file is not valid, refresh immediately
            if (!IsCacheValid())
            {
                await RefreshFeeRateAsync(s_cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // TODO: Task was canceled - this is expected but should log anyway
        }
        catch (Exception e)
        {
            throw new CriticalException("Unable to run FeeService background task.", e);
        }
    }

    public async Task<ulong> GetFeeRatePerKwAsync(CancellationToken cancellationToken = default)
    {
        if (IsCacheValid())
        {
            return s_cachedFeeRate;
        }

        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, s_cts?.Token ?? default);

        await RefreshFeeRateAsync(linkedCts.Token);
        return s_cachedFeeRate;
    }

    public ulong GetCachedFeeRatePerKw()
    {
        return s_cachedFeeRate;
    }

    public async Task RefreshFeeRateAsync(CancellationToken cancellationToken)
    {
        try
        {
            var feeRate = await FetchFeeRateFromApiAsync(cancellationToken);
            s_cachedFeeRate = feeRate;
            s_lastFetchTime = DateTime.UtcNow;
            await SaveToFileAsync(cancellationToken);
        }
        catch (Exception)
        {
            // If fetch fails and we already have a cached value, keep using it
            // TODO: Log the error
        }
    }

    public void Dispose()
    {
        s_cts?.Cancel();
        s_cts?.Dispose();
    }

    private async Task<ulong> FetchFeeRateFromApiAsync(CancellationToken cancellationToken)
    {
        HttpResponseMessage response;

        if (ConfigManager.Instance.FeeEstimationMethod.Equals("GET", StringComparison.CurrentCultureIgnoreCase))
        {
            response = await _httpClient.GetAsync(ConfigManager.Instance.FeeEstimationUrl, cancellationToken);
        }
        else // POST
        {
            var content = new StringContent(
                ConfigManager.Instance.FeeEstimationBody,
                System.Text.Encoding.UTF8,
                ConfigManager.Instance.FeeEstimationContentType);

            response = await _httpClient.PostAsync(ConfigManager.Instance.FeeEstimationUrl, content, cancellationToken);
        }

        response.EnsureSuccessStatusCode();
        var jsonResponseStream = await response.Content.ReadAsStreamAsync(cancellationToken);

        // Parse the JSON response
        using var document = await JsonDocument.ParseAsync(jsonResponseStream, cancellationToken: cancellationToken);
        var root = document.RootElement;

        // Extract the preferred fee rate from the JSON response
        if (!root.TryGetProperty(ConfigManager.Instance.PreferredFeeRate, out var feeRateElement))
        {
            throw new InvalidOperationException($"Could not extract {ConfigManager.Instance.PreferredFeeRate} from API response.");
        }

        // Parse the fee rate value
        if (!feeRateElement.TryGetDecimal(out var feeRate))
        {
            throw new InvalidOperationException($"Could not extract {ConfigManager.Instance.PreferredFeeRate} from API response.");
        }

        // Apply the multiplier to convert to sat/kw
        if (decimal.TryParse(ConfigManager.Instance.FeeRateMultiplier, out var multiplier))
        {
            return (ulong)(feeRate * multiplier);
        }

        throw new InvalidOperationException($"Could not extract {ConfigManager.Instance.PreferredFeeRate} from API response.");
    }

    private async Task RunPeriodicRefreshAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Wait for the cache time or until cancellation
                await Task.Delay(_cacheTimeExpiration, cancellationToken);

                // Refresh if not canceled
                if (!cancellationToken.IsCancellationRequested)
                {
                    await RefreshFeeRateAsync(cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // TODO: Task was canceled - this is expected but should log anyway
        }
        catch (Exception)
        {
            // TODO: Log any unexpected errors
        }
    }

    private async Task SaveToFileAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cacheData = new FeeRateCacheData
            {
                FeeRate = s_cachedFeeRate,
                LastFetchTime = s_lastFetchTime
            };

            await using var fileStream = File.OpenWrite(_cacheFilePath);
            await MessagePackSerializer.SerializeAsync(fileStream, cacheData, cancellationToken: cancellationToken);
        }
        catch (Exception)
        {
            // TODO: Log the error
        }
    }

    private async Task LoadFromFileAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_cacheFilePath))
            {
                return;
            }

            await using var fileStream = File.OpenRead(_cacheFilePath);
            var cacheData = await MessagePackSerializer.DeserializeAsync<FeeRateCacheData?>(fileStream, cancellationToken: cancellationToken);

            if (cacheData == null)
            {
                return;
            }

            s_cachedFeeRate = cacheData.FeeRate;
            s_lastFetchTime = cacheData.LastFetchTime;
        }
        catch (Exception)
        {
            // TODO: Log the error
        }
    }

    private bool IsCacheValid()
    {
        return s_cachedFeeRate > 0 && DateTime.UtcNow.Subtract(s_lastFetchTime).CompareTo(_cacheTimeExpiration) <= 0;
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

    private static string ParseFilePath()
    {
        var filePath = ConfigManager.Instance.FeeEstimationCacheFile;
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