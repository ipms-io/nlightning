namespace NLightning.Infrastructure.Crypto.Factories;

using Interfaces;
#if CRYPTO_JS
using Domain.Exceptions;
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
                return new Providers.JS.SodiumJsCryptoProvider();
            }
            else
            {
                throw new CriticalException("This provider is only available in a browser.");
            }
#elif CRYPTO_LIBSODIUM
            return new Providers.Libsodium.SodiumCryptoProvider();
#elif CRYPTO_NATIVE
        return new Providers.Native.NativeCryptoProvider();
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