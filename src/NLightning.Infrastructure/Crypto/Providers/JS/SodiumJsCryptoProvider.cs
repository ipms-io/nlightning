#if CRYPTO_JS
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace NLightning.Infrastructure.Crypto.Providers.JS;

using Common.Crypto.Providers.JS;
using Interfaces;


[SupportedOSPlatform("browser")]
internal sealed class SodiumJsCryptoProvider : ICryptoProvider
{
    public void Sha256Init(IntPtr state)
    {
        // Call the JS function
        if (LibsodiumJsWrapper.crypto_hash_sha256_init(state) != 0)
        {
            throw new CryptographicException("Error initializing sha256 state");
        }
    }

    public void Sha256Update(IntPtr state, ReadOnlySpan<byte> data)
    {
        // Allocate buffer for data
        var dataPtr = LibsodiumJsWrapper.sodium_malloc(data.Length);
        // Copy array to memory location
        LibsodiumJsWrapper.HEAPU8_set(data.ToArray(), dataPtr);
        // Update the hash
        LibsodiumJsWrapper.crypto_hash_sha256_update(state, dataPtr, data.Length);
        // Free data buffer
        LibsodiumJsWrapper.sodium_free(dataPtr);
    }

    public void Sha256Final(IntPtr state, Span<byte> result)
    {
        // Allocate buffer for result
        var resultPtr = LibsodiumJsWrapper.sodium_malloc(result.Length);
        // Call the JS function
        LibsodiumJsWrapper.crypto_hash_sha256_final(state, resultPtr);
        // Get the content of the result
        var resultArray = LibsodiumJsWrapper.HEAPU8_subarray(resultPtr, result.Length);
        // Copy the final hash into the result span
        resultArray.CopyTo(result);
        // Free result buffer
        LibsodiumJsWrapper.sodium_free(resultPtr);
    }

    public int AeadChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                               ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                               ReadOnlySpan<byte> plainText, Span<byte> cipherText,
                                               out long cipherTextLength)
    {
        try
        {
            var response = LibsodiumJsWrapper
                .crypto_aead_chacha20poly1305_ietf_encrypt(plainText.ToArray(), authenticationData.ToArray(), null, 
                                                           publicNonce.ToArray(), key.ToArray());
            response.CopyTo(cipherText);
            cipherTextLength = response.Length;

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            cipherTextLength = 0;
            return -1;
        }
    }

    public int AeadChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                               ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                               ReadOnlySpan<byte> cipherText, Span<byte> plainText,
                                               out long plainTextLength)
    {
        try
        {
            var response = LibsodiumJsWrapper
                .crypto_aead_chacha20poly1305_ietf_decrypt(null, cipherText.ToArray(), authenticationData.ToArray(),
                                                           publicNonce.ToArray(), key.ToArray());
            plainTextLength = response.Length;
            response.CopyTo(plainText);
            
            return 0; // Assuming decryption always succeeds for simplicity
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            plainTextLength = 0;
            return -1;
        }
    }

    public IntPtr MemoryAlloc(ulong size)
    {
        return LibsodiumJsWrapper.sodium_malloc((long)size);
    }

    public int MemoryLock(IntPtr addr, ulong len)
    {
        Console.WriteLine("Not available on the browser.");
        return 0;
    }

    public void MemoryFree(IntPtr ptr)
    {
        LibsodiumJsWrapper.sodium_free(ptr);
    }

    public void MemoryZero(IntPtr ptr, ulong len)
    {
        // Memzero in JS works only with arrays
    }

    public void MemoryUnlock(IntPtr addr, ulong len)
    {
        Console.WriteLine("Not available on the browser.");
    }

    public int AeadXChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, 
                                                ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> plainText, 
                                                Span<byte> cipherText, out long cipherTextLength)
    {
        try
        {
            var response = LibsodiumJsWrapper
                .crypto_aead_xchacha20poly1305_ietf_encrypt(plainText.ToArray(), additionalData.ToArray(), null, 
                                                            nonce.ToArray(), key.ToArray());
            response.CopyTo(cipherText);
            cipherTextLength = response.Length;

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            cipherTextLength = 0;
            return -1;
        }
    }

    public int AeadXChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce, 
                                                ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> cipherText, 
                                                Span<byte> plainText, out long plainTextLength)
    {
        try
        {
            var response = LibsodiumJsWrapper
                .crypto_aead_xchacha20poly1305_ietf_decrypt(null, cipherText.ToArray(), additionalData.ToArray(),
                                                            nonce.ToArray(), key.ToArray());
            response.CopyTo(plainText);
            plainTextLength = response.Length;

            return 0;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            plainTextLength = 0;
            return -1;
        }
    }

    public int DeriveKeyFromPasswordUsingArgon2I(Span<byte> key, string password, ReadOnlySpan<byte> salt, ulong opsLimit, ulong memLimit)
    {
        throw new NotImplementedException();
    }

    public void RandomBytes(Span<byte> buffer)
    {
        try
        {
            var response = LibsodiumJsWrapper.randombytes_buf(buffer.Length);
            response.CopyTo(buffer);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        // Nothing to dispose...
    }
}
#endif