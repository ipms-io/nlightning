using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq.Protected;

namespace NLightning.Common.Tests.Services;

using Common.Services;
using Common.Types;
using Options;

[Collection(TestCollections.SerialTestCollection.NAME)]
public class FeeServiceTests
{
    [Fact]
    public async Task GivenValidCache_WhenGetFeeRatePerKwAsync_ThenReturnsCachedValue()
    {
        // Arrange
        var feeService = new FeeService(new OptionsWrapper<FeeEstimationOptions>(new FeeEstimationOptions()),
            new HttpClient(new Mock<HttpMessageHandler>().Object),
            new Mock<ILogger<FeeService>>().Object);
        var cachedFeeRate = LightningMoney.Satoshis(1000);
        var cachedFeeRateField = typeof(FeeService).GetField("s_cachedFeeRate",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(cachedFeeRateField);
        cachedFeeRateField.SetValue(null, cachedFeeRate);
        var lastFetchTimeField = typeof(FeeService).GetField("s_lastFetchTime",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(lastFetchTimeField);
        lastFetchTimeField.SetValue(null, DateTime.UtcNow);
        var ctsField = typeof(FeeService).GetField("s_cts", System.Reflection.BindingFlags.NonPublic
                                                            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(ctsField);
        ctsField.SetValue(null, null);

        // Act
        var result = await feeService.GetFeeRatePerKwAsync();

        // Assert
        Assert.Equal(cachedFeeRate, result);
    }

    [Fact]
    public async Task GivenInvalidCache_WhenGetFeeRatePerKwAsync_ThenRefreshesAndReturnsNewValue()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(() => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"fastestFee\": 2}")
            });
        var feeService = new FeeService(new OptionsWrapper<FeeEstimationOptions>(new FeeEstimationOptions()),
            new HttpClient(httpMessageHandlerMock.Object),
            new Mock<ILogger<FeeService>>().Object);
        var lastFetchTimeField = typeof(FeeService).GetField("s_lastFetchTime",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(lastFetchTimeField);
        lastFetchTimeField.SetValue(null, DateTime.UtcNow.Subtract(TimeSpan.FromDays(1)));
        var ctsField = typeof(FeeService).GetField("s_cts", System.Reflection.BindingFlags.NonPublic
                                                            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(ctsField);
        ctsField.SetValue(null, null);
        // Act
        var result = await feeService.GetFeeRatePerKwAsync();

        // Assert
        Assert.Equal(2000, result.Satoshi);
    }

    [Fact]
    public async Task GivenApiFails_WhenRefreshFeeRateAsync_ThenUsesCachedValue()
    {
        // Arrange
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException());
        var feeService = new FeeService(new OptionsWrapper<FeeEstimationOptions>(new FeeEstimationOptions()),
            new HttpClient(new Mock<HttpMessageHandler>().Object),
            new Mock<ILogger<FeeService>>().Object);
        var expectedCachedFeeRate = LightningMoney.Satoshis(1000);
        var cachedFeeRateField = typeof(FeeService).GetField("s_cachedFeeRate",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(cachedFeeRateField);
        cachedFeeRateField.SetValue(null, expectedCachedFeeRate);
        var ctsField = typeof(FeeService).GetField("s_cts", System.Reflection.BindingFlags.NonPublic
                                                            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(ctsField);
        ctsField.SetValue(null, null);

        // Act
        var cachedFeeRate = await feeService.GetFeeRatePerKwAsync(CancellationToken.None);

        // Assert
        Assert.Equal(expectedCachedFeeRate, cachedFeeRate);
    }

    [Fact]
    public async Task GivenValidCache_WhenRefreshFeeRateAsync_ThenSavesCacheToFile()
    {
        // Arrange
        var feeService = new FeeService(new OptionsWrapper<FeeEstimationOptions>(new FeeEstimationOptions()),
            new HttpClient(new Mock<HttpMessageHandler>().Object),
            new Mock<ILogger<FeeService>>().Object);
        var tempFilePath = Path.GetTempFileName();
        var cacheFilePathField = typeof(FeeService).GetField("_cacheFilePath",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Instance);
        Assert.NotNull(cacheFilePathField);
        cacheFilePathField.SetValue(feeService, tempFilePath);
        var feeRate = LightningMoney.Satoshis(1500);
        var cachedFeeRateField = typeof(FeeService).GetField("s_cachedFeeRate",
            System.Reflection.BindingFlags.NonPublic
            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(cachedFeeRateField);
        cachedFeeRateField.SetValue(null, feeRate);
        var ctsField = typeof(FeeService).GetField("s_cts", System.Reflection.BindingFlags.NonPublic
                                                            | System.Reflection.BindingFlags.Static);
        Assert.NotNull(ctsField);
        ctsField.SetValue(null, null);

        // Act
        await feeService.RefreshFeeRateAsync(CancellationToken.None);

        // Assert
        Assert.True(File.Exists(tempFilePath));
        File.Delete(tempFilePath);
    }

    [Fact]
    public async Task GivenValidConfig_WhenStartAsync_ThenRefreshesFeeRatePeriodically()
    {
        // Arrange

        var refreshCount = 0;
        var cancellationTokenSource = new CancellationTokenSource();
        var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"fastestFee\": 2}")
            })
            .Callback(() =>
            {
                refreshCount++;
                if (refreshCount >= 3)
                {
                    cancellationTokenSource.Cancel();
                }
            });
        var feeOptions = new FeeEstimationOptions
        {
            CacheExpiration = "1s"
        };
        var feeService = new FeeService(new OptionsWrapper<FeeEstimationOptions>(feeOptions),
                                        new HttpClient(httpMessageHandlerMock.Object),
                                        new Mock<ILogger<FeeService>>().Object);

        try
        {
            var cacheTimeExpirationField = typeof(FeeService).GetField("_cacheTimeExpiration",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(cacheTimeExpirationField);
            cacheTimeExpirationField.SetValue(feeService, TimeSpan.FromMilliseconds(100));

            // Act
            var startRefreshTask = feeService.StartAsync(cancellationTokenSource.Token);
            var backgroundTaskField = typeof(FeeService).GetField("s_backgroundTask",
                System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Static);
            Assert.NotNull(backgroundTaskField);
            var backgroundTask = backgroundTaskField.GetValue(null) as Task
                                 ?? throw new Exception("Can't find field to test.");

            await startRefreshTask;
            await backgroundTask;

            // Assert
            Assert.True(refreshCount >= 3);
        }
        finally
        {
            await feeService.StopAsync();
        }
    }
}