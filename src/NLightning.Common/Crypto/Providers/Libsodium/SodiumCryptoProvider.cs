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

    public int AeadChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
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

    public int AeadChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
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

    public int AeadXChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
                                                ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> plainText,
                                                Span<byte> cipherText, out long cipherTextLength)
    {
        return LibsodiumWrapper
            .crypto_aead_xchacha20poly1305_ietf_encrypt(ref MemoryMarshal.GetReference(cipherText),
                                                        out cipherTextLength, ref MemoryMarshal.GetReference(plainText),
                                                        plainText.Length,
                                                        ref MemoryMarshal.GetReference(additionalData),
                                                        additionalData.Length, IntPtr.Zero,
                                                        ref MemoryMarshal.GetReference(nonce),
                                                        ref MemoryMarshal.GetReference(key));
    }

    public int AeadXChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
                                                ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> cipherText,
                                                Span<byte> plainText, out long plainTextLength)
    {
        return LibsodiumWrapper
            .crypto_aead_xchacha20poly1305_ietf_decrypt(ref MemoryMarshal.GetReference(plainText),
                                                        out plainTextLength, IntPtr.Zero,
                                                        ref MemoryMarshal.GetReference(cipherText),
                                                        cipherText.Length,
                                                        ref MemoryMarshal.GetReference(additionalData),
                                                        additionalData.Length,
                                                        ref MemoryMarshal.GetReference(nonce),
                                                        ref MemoryMarshal.GetReference(key));
    }

    public int DeriveKeyFromPasswordUsingArgon2I(Span<byte> key, string password, ReadOnlySpan<byte> salt, ulong opsLimit, ulong memLimit)
    {
        const int ALG = 2; // crypto_pwhash_ALG_ARGON2ID13
        return LibsodiumWrapper.crypto_pwhash(ref MemoryMarshal.GetReference(key), (ulong)key.Length, password,
                                              (ulong)password.Length, ref MemoryMarshal.GetReference(salt), opsLimit,
                                              memLimit, ALG);
    }

    public void RandomBytes(Span<byte> buffer)
    {
        LibsodiumWrapper.randombytes_buf(ref MemoryMarshal.GetReference(buffer), (UIntPtr)buffer.Length);
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