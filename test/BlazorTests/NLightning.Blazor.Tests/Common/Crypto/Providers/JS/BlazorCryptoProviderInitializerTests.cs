using NLightning.Blazor.Tests.Infrastructure;
using Xunit.Abstractions;

namespace NLightning.Blazor.Tests.Common.Crypto.Providers.JS;

public class BlazorCryptoProviderInitializer(ITestOutputHelper output) : BlazorTestBase(output)
{
    [Fact]
    public async Task GivenHomepage_WhenItLoads_ThenContentIsDisplayedCorrectly()
    {
        // Arrange
        Assert.NotNull(Page);

        // Act
        await Page.GotoAsync(ROOT_URI, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        // Assert
        var heading = await Page.QuerySelectorAsync("h1");
        Assert.NotNull(heading);
        var text = await heading.InnerTextAsync();
        Assert.Equal("Blazor Test App", text);
    }

    [Fact]
    public async Task GivenHomepage_WhenItLoads_ThenConsoleLogsExpectedMessage()
    {
        // Arrange
        Assert.NotNull(Page);
        ClearOutput();
        var console = new List<string>();
        Page.Console += (_, message) =>
        {
            console.Add(message.Text);
        };

        // Act
        await Page.GotoAsync(ROOT_URI, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        // Assert
        Assert.NotEmpty(console);
        Assert.Contains("Sodium init: { version: 1.0.20, wasm: false }", console);
    }
}