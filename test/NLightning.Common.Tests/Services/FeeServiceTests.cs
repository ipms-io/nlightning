using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace NLightning.Common.Tests.Services;

using Common.Services;
using Common.Types;
using Options;

public class FeeServiceTests
{
    private static Mock<HttpMessageHandler>? s_httpMessageHandlerMock;
    private static FeeService? s_feeService;

    public FeeServiceTests()
    {
        if (s_feeService is not null)
        {
            return;
        }

        s_httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(s_httpMessageHandlerMock.Object);
        s_feeService = new FeeService(new OptionsWrapper<FeeEstimationOptions>(new FeeEstimationOptions()), httpClient,
                                      new Mock<ILogger<FeeService>>().Object);
    }

    [Fact]
    public async Task GetFeeRatePerKwAsync_WhenCacheIsValid_ReturnsCachedValue()
    {
        // Arrange
        var cachedFeeRate = LightningMoney.Satoshis(1000);
        typeof(FeeService).GetField("s_cachedFeeRate", System.Reflection.BindingFlags.NonPublic
                                                       | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, cachedFeeRate);
        typeof(FeeService).GetField("s_lastFetchTime", System.Reflection.BindingFlags.NonPublic
                                                       | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, DateTime.UtcNow);

        // Act
        var result = await s_feeService!.GetFeeRatePerKwAsync();

        // Assert
        Assert.Equal(cachedFeeRate, result);
    }

    [Fact]
    public async Task GetFeeRatePerKwAsync_WhenCacheIsInvalid_RefreshesAndReturnsNewValue()
    {
        // Arrange
        typeof(FeeService).GetField("s_lastFetchTime", System.Reflection.BindingFlags.NonPublic
                                                       | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, DateTime.UtcNow.Subtract(TimeSpan.FromHours(1)));
        const string API_RESPONSE = "{\"fastestFee\": 2}";
        s_httpMessageHandlerMock!.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(API_RESPONSE)
            });

        // Act
        var result = await s_feeService!.GetFeeRatePerKwAsync();

        // Assert
        Assert.Equal(2000, result.Satoshi);
    }

    [Fact]
    public async Task RefreshFeeRateAsync_WhenApiFails_UsesCachedValue()
    {
        // Arrange
        var cachedFeeRate = LightningMoney.Satoshis(1000);
        typeof(FeeService).GetField("s_cachedFeeRate", System.Reflection.BindingFlags.NonPublic
                                                       | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, cachedFeeRate);

        s_httpMessageHandlerMock!.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());

        // Act
        await s_feeService!.RefreshFeeRateAsync(CancellationToken.None);

        // Assert
        Assert.Equal(cachedFeeRate, s_feeService.GetCachedFeeRatePerKw());
    }

    [Fact]
    public async Task SaveToFileAsync_WhenCalled_SavesCacheToFile()
    {
        // Arrange
        var tempFilePath = Path.GetTempFileName();
        typeof(FeeService).GetField("_cacheFilePath", System.Reflection.BindingFlags.NonPublic
                                                      | System.Reflection.BindingFlags.Instance)
            ?.SetValue(s_feeService, tempFilePath);

        var feeRate = LightningMoney.Satoshis(1500);
        typeof(FeeService).GetField("s_cachedFeeRate", System.Reflection.BindingFlags.NonPublic
                                                       | System.Reflection.BindingFlags.Static)
            ?.SetValue(null, feeRate);

        // Act
        await s_feeService!.RefreshFeeRateAsync(CancellationToken.None);

        // Assert
        Assert.True(File.Exists(tempFilePath));
        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task RunPeriodicRefreshAsync_RefreshesFeeRatePeriodically()
    {
        // Arrange
        var refreshCount = 0;
        var cancellationTokenSource = new CancellationTokenSource();
        typeof(FeeService).GetField("_cacheTimeExpiration", System.Reflection.BindingFlags.NonPublic
                                                            | System.Reflection.BindingFlags.Instance)
            ?.SetValue(s_feeService, TimeSpan.FromMilliseconds(100));

        s_httpMessageHandlerMock!.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                                              ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"preferredFeeRate\": 2000}")
            })
            .Callback(() =>
            {
                refreshCount++;
                if (refreshCount >= 3)
                {
                    cancellationTokenSource.Cancel();
                }
            });

        // Act
        var startRefreshTask = s_feeService!.StartAsync(cancellationTokenSource.Token);
        var backgroundTask = typeof(FeeService).GetField("s_backgroundTask", System.Reflection.BindingFlags.NonPublic
                                                                             | System.Reflection.BindingFlags.Static)
            ?.GetValue(s_feeService) as Task ?? throw new Exception("Can't find field to test.");

        await startRefreshTask;
        await backgroundTask;

        // Assert
        Assert.True(refreshCount >= 3);
    }
}