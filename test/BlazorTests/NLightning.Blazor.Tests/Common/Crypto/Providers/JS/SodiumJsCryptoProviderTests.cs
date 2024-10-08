using Newtonsoft.Json;

namespace NLightning.Blazor.Tests.Common.Crypto.Providers.JS;

using Helpers;
using Infrastructure;
using NLightning.Common.Tests.Vectors;

[Collection("Blazor Test Collection")]
public class SodiumJsCryptoProviderTests : BlazorTestBase
{
    [Fact]
    public async Task GivenAeadChacha20Poly1305IetfTestsPage_WhenEncodeButtonIsClicked_ThenConsoleLogsExpectedResult()
    {
        // Arrange
        Assert.NotNull(Page);
        ClearOutput();
        await Page.GotoAsync(ROOT_URI, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        // Act
        var a = Page.GetByTestId("encode");
        Assert.NotNull(a);
        await a.ClickAsync();

        await Page.WaitForSelectorAsync("[data-testid='result']", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        // Assert
        var resultElement = Page.GetByTestId("result");
        Assert.NotNull(resultElement);
        var resultJson = await resultElement.TextContentAsync();
        Assert.NotNull(resultJson);
        var result = JsonHelper.Deserialize<AeadChacha20Poly1305IetfEncodeResult>(resultJson);
        Assert.NotNull(result?.Result);
        Assert.Equal(AeadChacha20Poly1305IetfVector.CIPHER.Length, result.Result.ClenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.CIPHER, result.Result.CipherBytes);
    }

    public class AeadChacha20Poly1305IetfEncodeResult
    {
        public ResultData? Result { get; set; }

        public class ResultData
        {
            public int ClenP { get; set; }
            public string? Cipher { get; set; }

            [JsonIgnore]
            public byte[] CipherBytes => Convert.FromHexString(Cipher ?? throw new NullReferenceException("Cipher is null."));
        }
    }
}