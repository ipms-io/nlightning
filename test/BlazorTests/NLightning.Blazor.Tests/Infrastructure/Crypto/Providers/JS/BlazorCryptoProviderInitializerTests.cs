using Xunit.Abstractions;

namespace NLightning.Blazor.Tests.Infrastructure.Crypto.Providers.JS;

using TestCollections;

[Collection(BlazorTestCollection.Name)]
public class BlazorCryptoProviderInitializer : BlazorTestBase
{
    private readonly ITestOutputHelper _testOutputHelper;

    public BlazorCryptoProviderInitializer(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task GivenHomepage_WhenItLoads_ThenContentIsDisplayedCorrectly()
    {
        // Arrange
        Assert.NotNull(Page);

        _testOutputHelper.WriteLine("=== BASE CONSTRUCTOR MESSAGES ===");
        _testOutputHelper.WriteLine(ConsoleOutput.GetStringBuilder().ToString());
        _testOutputHelper.WriteLine("=== END BASE CONSTRUCTOR MESSAGES ===");

        // Enable console logging to see what's happening
        var consoleMessages = new List<string>();
        Page.Console += (_, message) =>
        {
            if (message.Type == "error") consoleMessages.Add($"{message.Type}: {message.Text}");
        };

        // Track network requests
        var networkRequests = new List<string>();
        Page.Request += (_, request) => networkRequests.Add($"REQUEST: {request.Method} {request.Url}");
        Page.Response += (_, response) => networkRequests.Add($"RESPONSE: {response.Status} {response.Url}");

        // Make sure the page is fresh
        await Page.GotoAsync("about:blank", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // Act
        try
        {
            await Page.GotoAsync(RootUri, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 5000
            });

            // Check if the page loaded at all
            var pageTitle = await Page.TitleAsync();
            Assert.NotNull(pageTitle);

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
        catch (Exception ex)
        {
            // Log everything for debugging
            _testOutputHelper.WriteLine("=== TEST FAILURE DEBUG ===");

            _testOutputHelper.WriteLine("Console messages:");
            foreach (var msg in consoleMessages)
                _testOutputHelper.WriteLine($"  {msg}");

            _testOutputHelper.WriteLine("Network requests:");
            foreach (var req in networkRequests)
                _testOutputHelper.WriteLine($"  {req}");

            _testOutputHelper.WriteLine("=== END DEBUG ===");

            // Log console messages and page content for debugging
            throw new Exception(
                $"Test failed. Console messages: {string.Join(", ", consoleMessages)}. Network requests: {string.Join(", ", networkRequests)}",
                ex);
        }
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