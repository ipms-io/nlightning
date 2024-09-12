#if CRYPTO_JS
using System.Runtime.InteropServices.JavaScript;

namespace NLightning.Common.Crypto.Providers.JS;

public static class BlazorCryptoProvider
{
    public static async Task InitializeBlazorCryptoProviderAsync()
    {
        if (!OperatingSystem.IsBrowser())
        {
            throw new InvalidOperationException("You can only initialize this in a browser.");
        }

        var assemblyName = typeof(LibsodiumJsWrapper).Assembly.GetName().Name;
        var scriptPath = $"../_content/{assemblyName}/blazorSodium.bundle.js";
        
        await JSHost.ImportAsync("blazorSodium", scriptPath);
        await LibsodiumJsWrapper.InitializeAsync();
    }
}
#endif