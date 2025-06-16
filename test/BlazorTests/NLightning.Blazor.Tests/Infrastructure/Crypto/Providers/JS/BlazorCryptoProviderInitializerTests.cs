namespace NLightning.Blazor.Tests.Infrastructure.Crypto.Providers.JS;

[Collection("Blazor Test Collection")]
public class BlazorCryptoProviderInitializer : BlazorTestBase
{
    [Fact]
    public async Task GivenHomepage_WhenItLoads_ThenContentIsDisplayedCorrectly()
    {
        // Arrange
        Assert.NotNull(Page);
        // Make sure the page is fresh
        await Page.GotoAsync("about:blank", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Act
        await Page.GotoAsync(RootUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        await Page.WaitForSelectorAsync("h1", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
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
        await Page.GotoAsync("about:blank",
                             new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle }); // Make sure page is fresh
        var console = new List<string>();
        Page.Console += ConsoleListener;

        // Act
        await Page.GotoAsync(RootUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        await Page.WaitForSelectorAsync("h1", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        // Assert
        Assert.NotEmpty(console);
        Assert.Contains("Sodium init: { version: 1.0.20, wasm: false }", console);

        // Cleanup
        Page.Console -= ConsoleListener;
        return;

        void ConsoleListener(object? _, IConsoleMessage message)
        {
            console.Add(message.Text);
        }
    }
}