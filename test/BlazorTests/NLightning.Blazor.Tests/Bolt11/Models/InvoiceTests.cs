namespace NLightning.Blazor.Tests.Bolt11.Models;

using Helpers;
using Infrastructure;
using TestCollections;

[Collection(BlazorTestCollection.Name)]
public class InvoiceTests : BlazorTestBase
{
    [Fact]
    public async Task GivenExampleInvoice_WhenDecodeButtonIsClicked_ThenInvoiceIsDecodedCorrectly()
    {
        Assert.NotNull(Page);
        ClearOutput();
        await Page.GotoAsync(RootUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        var button = Page.GetByTestId("decodeExampleInvoice");
        Assert.NotNull(button);
        await button.ClickAsync();

        await Page.WaitForSelectorAsync("[data-testid='decodeInvoiceResult']",
                                        new PageWaitForSelectorOptions
                                        {
                                            State = WaitForSelectorState.Visible,
                                            Timeout = 5000
                                        });

        // Assert
        var resultElement = Page.GetByTestId("decodeInvoiceResult");
        Assert.NotNull(resultElement);
        var resultJson = await resultElement.TextContentAsync();
        Assert.NotNull(resultJson);
        var result = JsonHelper.Deserialize<DecodedInvoice>(resultJson);
        Assert.NotNull(result?.Result);
        Assert.Null(result.Result.Error);
        Assert.Equal(152000u, result.Result.Amount);
    }

    public class DecodedInvoice
    {
        public ResultData? Result { get; set; }

        public class ResultData
        {
            public string? Error { get; set; }
            public ulong Amount { get; set; }
        }
    }
}