namespace NLightning.Common.Factories.Crypto;

using Common.Interfaces.Crypto;
#if CRYPTO_JS
using Exceptions;
#endif

internal static class CryptoFactory
{
    public static ICryptoProvider GetCryptoProvider()
    {
        try
        {
#if CRYPTO_JS
            if (OperatingSystem.IsBrowser())
            {
                return new Common.Crypto.Providers.JS.SodiumJsCryptoProvider();
            }
            else
            {
                throw new CriticalException("This provider is only available in a browser.");
            }
#elif CRYPTO_LIBSODIUM
            return new Common.Crypto.Providers.Libsodium.SodiumCryptoProvider();
#elif CRYPTO_NATIVE
        return new Common.Crypto.Providers.Native.NativeCryptoProvider();
#else
        throw new NotImplementedException("You have to set one of the compiler flags in order to use this factory.");
#endif
        }
        catch (Exception e)
        {
            Console.Write(e.StackTrace);
            throw;
        }
    }
}