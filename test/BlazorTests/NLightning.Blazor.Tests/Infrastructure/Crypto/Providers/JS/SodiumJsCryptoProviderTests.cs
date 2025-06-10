using Newtonsoft.Json;
using NLightning.Blazor.Tests.Helpers;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Blazor.Tests.Infrastructure.Crypto.Providers.JS;

[Collection("Blazor Test Collection")]
public class SodiumJsCryptoProviderTests : BlazorTestBase
{
    [Fact]
    public async Task GivenAeadChacha20Poly1305IetfTestsPage_WhenEncodeButtonIsClicked_ThenConsoleLogsExpectedResult()
    {
        // Arrange
        Assert.NotNull(Page);
        ClearOutput();
        await Page.GotoAsync(RootUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        // Act
        var a = Page.GetByTestId("encodeAeadChacha20Poly1305Ietf");
        Assert.NotNull(a);
        await a.ClickAsync();

        await Page.WaitForSelectorAsync("[data-testid='aeadChacha20Poly1305IetfResult']", new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 5000
        });

        // Assert
        var resultElement = Page.GetByTestId("aeadChacha20Poly1305IetfResult");
        Assert.NotNull(resultElement);
        var resultJson = await resultElement.TextContentAsync();
        Assert.NotNull(resultJson);
        var result = JsonHelper.Deserialize<EncodeResult>(resultJson);
        Assert.NotNull(result?.Result);
        Assert.Equal(AeadChacha20Poly1305IetfVector.Cipher.Length, result.Result.ClenP);
        Assert.Equal(AeadChacha20Poly1305IetfVector.Cipher, result.Result.CipherBytes);
    }

    [Fact]
    public async Task GivenAeadXChacha20Poly1305IetfTestsPage_WhenEncodeButtonIsClicked_ThenConsoleLogsExpectedResult()
    {
        // Arrange
        Assert.NotNull(Page);
        ClearOutput();
        await Page.GotoAsync(RootUri, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 5000
        });

        // Act
        var a = Page.GetByTestId("encodeAeadXChacha20Poly1305Ietf");
        Assert.NotNull(a);
        await a.ClickAsync();

        await Page.WaitForSelectorAsync("[data-testid='aeadXChacha20Poly1305IetfResult']",
                                        new PageWaitForSelectorOptions
                                        {
                                            State = WaitForSelectorState.Visible,
                                            Timeout = 5000
                                        });

        // Assert
        var resultElement = Page.GetByTestId("aeadXChacha20Poly1305IetfResult");
        Assert.NotNull(resultElement);
        var resultJson = await resultElement.TextContentAsync();
        Assert.NotNull(resultJson);
        var result = JsonHelper.Deserialize<EncodeResult>(resultJson);
        Assert.NotNull(result?.Result);
        Assert.Equal(AeadXChacha20Poly1305IetfVector.Cipher.Length, result.Result.ClenP);
        Assert.Equal(AeadXChacha20Poly1305IetfVector.Cipher, result.Result.CipherBytes);
    }

    public class EncodeResult
    {
        public ResultData? Result { get; set; }

        public class ResultData
        {
            public int ClenP { get; set; }
            public string? Cipher { get; set; }

            [JsonIgnore]
            public byte[] CipherBytes =>
                Convert.FromHexString(Cipher ?? throw new NullReferenceException("Cipher is null."));
        }
    }
}