#if CRYPTO_LIBSODIUM
using System.Runtime.InteropServices;

namespace NLightning.Common.Crypto.Providers.Libsodium;

using Interfaces.Crypto;

internal sealed class SodiumCryptoProvider : ICryptoProvider
{
    public void Sha256Init(IntPtr state)
    {
        _ = LibsodiumWrapper.crypto_hash_sha256_init(state);
    }

    public void Sha256Update(IntPtr state, ReadOnlySpan<byte> data)
    {
        _ = LibsodiumWrapper.crypto_hash_sha256_update(state, ref MemoryMarshal.GetReference(data), (ulong)data.Length);
    }

    public void Sha256Final(IntPtr state, Span<byte> result)
    {
        _ = LibsodiumWrapper.crypto_hash_sha256_final(state, ref MemoryMarshal.GetReference(result));
    }

    public int AeadChacha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                               ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                               ReadOnlySpan<byte> message, Span<byte> cipher, out long cipherLength)
    {
        return LibsodiumWrapper.crypto_aead_chacha20poly1305_ietf_encrypt(
            ref MemoryMarshal.GetReference(cipher),
            out cipherLength,
            ref MemoryMarshal.GetReference(message),
            message.Length,
            ref MemoryMarshal.GetReference(authenticationData),
            authenticationData.Length,
            IntPtr.Zero,
            ref MemoryMarshal.GetReference(publicNonce),
            ref MemoryMarshal.GetReference(key)
        );
    }

    public int AeadChacha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                               ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                               ReadOnlySpan<byte> cipher, Span<byte> clearTextMessage,
                                               out long messageLength)
    {
        return LibsodiumWrapper.crypto_aead_chacha20poly1305_ietf_decrypt(
            ref MemoryMarshal.GetReference(clearTextMessage),
            out messageLength,
            IntPtr.Zero,
            ref MemoryMarshal.GetReference(cipher),
            cipher.Length,
            ref MemoryMarshal.GetReference(authenticationData),
            authenticationData.Length,
            ref MemoryMarshal.GetReference(publicNonce),
            ref MemoryMarshal.GetReference(key)
        );
    }

    public IntPtr MemoryAlloc(ulong size)
    {
        return LibsodiumWrapper.sodium_malloc(size);
    }

    public int MemoryLock(IntPtr addr, ulong len)
    {
        return LibsodiumWrapper.sodium_mlock(addr, len);
    }

    public void MemoryFree(IntPtr ptr)
    {
        LibsodiumWrapper.sodium_free(ptr);
    }

    public void MemoryZero(IntPtr ptr, ulong len)
    {
        LibsodiumWrapper.sodium_memzero(ptr, len);
    }

    public void MemoryUnlock(IntPtr addr, ulong len)
    {
        LibsodiumWrapper.sodium_munlock(addr, len);
    }

    public void Dispose()
    {
        // There's no managed resources to free
    }
}
#endif