#if CRYPTO_NATIVE
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using Org.BouncyCastle.Crypto.Parameters;

namespace NLightning.Infrastructure.Crypto.Providers.Native;

using Ciphers;
using Constants;
using Domain.Crypto.Constants;
using Interfaces;

internal sealed partial class NativeCryptoProvider : ICryptoProvider
{
    private readonly IncrementalHash _sha256 = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

    public void Sha256Init(IntPtr state)
    {
        // There's no need to initialize it here, since if it was used before, it was already reseted
    }

    public void Sha256Update(IntPtr state, ReadOnlySpan<byte> data)
    {
        _sha256.AppendData(data.ToArray());
    }

    public void Sha256Final(IntPtr state, Span<byte> result)
    {
        _ = _sha256.GetHashAndReset(result);
    }

    public int AeadChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                               ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                               ReadOnlySpan<byte> message, Span<byte> cipher, out long cipherLength)
    {
        try
        {
            using var chaCha20Poly1305 = new ChaCha20Poly1305(key);

            chaCha20Poly1305.Encrypt(publicNonce, message, cipher[..message.Length],
                                     cipher[message.Length..(message.Length + CryptoConstants.Chacha20Poly1305TagLen)],
                                     authenticationData);

            cipherLength = message.Length + CryptoConstants.Chacha20Poly1305TagLen;

            return 0;
        }
        catch (Exception e)
        {
            throw new CryptographicException("Encryption failed.", e);
        }
    }

    public int AeadChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                               ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                               ReadOnlySpan<byte> cipher, Span<byte> clearTextMessage,
                                               out long messageLength)
    {
        try
        {
            using var chaCha20Poly1305 = new ChaCha20Poly1305(key);

            var messageLengthWithoutTag = cipher.Length - CryptoConstants.Chacha20Poly1305TagLen;

            chaCha20Poly1305.Decrypt(publicNonce, cipher[..messageLengthWithoutTag], cipher[messageLengthWithoutTag..],
                                     clearTextMessage[..messageLengthWithoutTag], authenticationData);

            messageLength = messageLengthWithoutTag;

            return 0;
        }
        catch (Exception e)
        {
            throw new CryptographicException("Decryption failed.", e);
        }
    }

    public IntPtr MemoryAlloc(ulong size)
    {
        return Marshal.AllocHGlobal((IntPtr)size);
    }

    public int MemoryLock(IntPtr addr, ulong len)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return VirtualLock(addr, len) ? 0 : Marshal.GetLastWin32Error();
        }

        // TODO: Log somewhere that Memory lock is not available on this platform.
        // but return success so the process can continue
        return 0;
    }

    public void MemoryUnlock(IntPtr addr, ulong len)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _ = VirtualUnlock(addr, len);
        }
        // else
        // {
        // TODO: Log somewhere that Memory unlock is not available on this platform.
        // but don't fail so the process can continue
        // }
    }

    public int AeadXChaCha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
                                                ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> plainText,
                                                Span<byte> cipherText, out long cipherTextLength)
    {
        try
        {
            if (key.Length != XChaCha20Constants.KeySize)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));

            if (nonce.Length != XChaCha20Constants.NonceSize)
                throw new ArgumentException("Nonce must be 24 bytes", nameof(nonce));

            // subkey (hchacha20(key, nonce[0:15]))
            Span<byte> subkey = stackalloc byte[XChaCha20Constants.SubkeySize];
            HChaCha20.CreateSubkey(key, nonce, subkey);

            // nonce (chacha20_nonce = "\x00\x00\x00\x00" + nonce[16:23])
            Span<byte> chaChaNonce = stackalloc byte[12];
            "\0\0\0\0"u8.ToArray().CopyTo(chaChaNonce[..4]);
            nonce[16..].CopyTo(chaChaNonce[4..]);

            // chacha20_encrypt(subkey, chacha20_nonce, plaintext, blk_ctr)
            var keyMaterial = new KeyParameter(subkey.ToArray());
            var parameters = new ParametersWithIV(keyMaterial, chaChaNonce.ToArray());

            var chaCha20Poly1305 = new Org.BouncyCastle.Crypto.Modes.ChaCha20Poly1305();
            chaCha20Poly1305.Init(true, parameters);

            // if additional data present
            if (additionalData != Span<byte>.Empty)
            {
                chaCha20Poly1305.ProcessAadBytes(additionalData.ToArray(), 0, additionalData.Length);
            }

            var cipherTextBytes = new byte[cipherText.Length + CryptoConstants.Xchacha20Poly1305TagLen];
            var len1 = chaCha20Poly1305.ProcessBytes(plainText.ToArray(), 0, plainText.Length, cipherTextBytes, 0);
            var len2 = chaCha20Poly1305.DoFinal(cipherTextBytes, len1);
            cipherTextLength = len1 + len2;

            cipherTextBytes.CopyTo(cipherText);

            return 0;
        }
        catch (Exception e)
        {
            throw new CryptographicException("Encryption failed.", e);
        }
    }

    public int AeadXChaCha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> nonce,
                                                ReadOnlySpan<byte> additionalData, ReadOnlySpan<byte> cipherText,
                                                Span<byte> plainText, out long plainTextLength)
    {
        try
        {
            if (key.Length != XChaCha20Constants.KeySize)
                throw new ArgumentException("Key must be 32 bytes", nameof(key));

            if (nonce.Length != XChaCha20Constants.NonceSize)
                throw new ArgumentException("Nonce must be 24 bytes", nameof(nonce));

            // subkey (hchacha20(key, nonce[0:15]))
            Span<byte> subkey = stackalloc byte[XChaCha20Constants.SubkeySize];
            HChaCha20.CreateSubkey(key, nonce, subkey);

            // nonce (chacha20_nonce = "\x00\x00\x00\x00" + nonce[16:23])
            Span<byte> chaChaNonce = stackalloc byte[12];
            "\0\0\0\0"u8.ToArray().CopyTo(chaChaNonce[..4]);
            nonce[16..].CopyTo(chaChaNonce[4..]);

            // chacha20_encrypt(subkey, chacha20_nonce, plaintext, blk_ctr)
            var keyMaterial = new KeyParameter(subkey.ToArray());
            var parameters = new ParametersWithIV(keyMaterial, chaChaNonce.ToArray());

            var chaCha20Poly1305 = new Org.BouncyCastle.Crypto.Modes.ChaCha20Poly1305();
            chaCha20Poly1305.Init(false, parameters);

            // if additional data present
            if (additionalData != Span<byte>.Empty)
                chaCha20Poly1305.ProcessAadBytes(additionalData.ToArray(), 0, additionalData.Length);

            var plainTextBytes = new byte[plainText.Length];
            var len1 = chaCha20Poly1305.ProcessBytes(cipherText.ToArray(), 0, cipherText.Length, plainTextBytes, 0);
            var len2 = chaCha20Poly1305.DoFinal(plainTextBytes, (int)len1);
            plainTextLength = len1 + len2;

            plainTextBytes.CopyTo(plainText);

            return 0;
        }
        catch (Exception e)
        {
            throw new CryptographicException("Decryption failed.", e);
        }
    }

    public int DeriveKeyFromPasswordUsingArgon2I(Span<byte> key, string password, ReadOnlySpan<byte> salt,
                                                 ulong opsLimit, ulong memLimit)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password));
        argon2.Salt = salt.ToArray();
        argon2.Iterations = (int)opsLimit;
        argon2.MemorySize = (int)(memLimit / 1024); // memLimit is in bytes, MemorySize is in KB
        argon2.DegreeOfParallelism = 1;

        var derived = argon2.GetBytes(key.Length);
        derived.CopyTo(key);
        return 0;
    }

    public void RandomBytes(Span<byte> buffer)
    {
        RandomNumberGenerator.Fill(buffer);
    }

    public void MemoryFree(IntPtr ptr)
    {
        Marshal.FreeHGlobal(ptr);
    }

    public void MemoryZero(IntPtr ptr, ulong len)
    {
        unsafe
        {
            var span = new Span<byte>((void*)ptr, (int)len);
            CryptographicOperations.ZeroMemory(span);
        }
    }

    // P/Invoke for Windows VirtualLock and VirtualUnlock
    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool VirtualLock(IntPtr lpAddress, ulong dwSize);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool VirtualUnlock(IntPtr lpAddress, ulong dwSize);

    public void Dispose()
    {
        _sha256.Dispose();
    }
}
#endif