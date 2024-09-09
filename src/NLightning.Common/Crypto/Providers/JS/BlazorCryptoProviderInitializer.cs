#if CRYPTO_JS
using System.Runtime.InteropServices.JavaScript;

namespace NLightning.Common.Crypto.Providers.JS;

public static class BlazorCryptoProvider
{
    public static async Task InitializeBlazorCryptoProviderAsync()
    {
        if (OperatingSystem.IsBrowser())
        {
            await JSHost.ImportAsync("blazorSodium", "../_content/NLightning.Bolt11.Blazor/blazorSodium.bundle.js");
            await LibsodiumJsWrapper.InitializeAsync();
        }
    }
}
#endif