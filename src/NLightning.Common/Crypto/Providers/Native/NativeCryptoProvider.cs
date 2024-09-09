#if CRYPTO_NATIVE
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Common.Crypto.Providers.Native;

using Constants;
using Interfaces.Crypto;

internal sealed partial class NativeCryptoProvider: ICryptoProvider
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

    public int AeadChacha20Poly1305IetfEncrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                                ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                                ReadOnlySpan<byte> message, Span<byte> cipher, out long cipherLength)
    {
        using var chaCha20Poly1305 = new ChaCha20Poly1305(key);
        
        // var tag = new byte[CryptoConstants.CHACHA20_POLY1305_TAG_LEN];
        chaCha20Poly1305.Encrypt(publicNonce, message, cipher[..message.Length], cipher[message.Length..], authenticationData);
        // tag.CopyTo(cipher[message.Length..]); // Append the tag at the end

        cipherLength = message.Length + CryptoConstants.CHACHA20_POLY1305_TAG_LEN;

        return 0;
    }

    public int AeadChacha20Poly1305IetfDecrypt(ReadOnlySpan<byte> key, ReadOnlySpan<byte> publicNonce,
                                                ReadOnlySpan<byte> secureNonce, ReadOnlySpan<byte> authenticationData,
                                                ReadOnlySpan<byte> cipher, Span<byte> clearTextMessage,
                                                out long messageLength)
    {
        using var chaCha20Poly1305 = new ChaCha20Poly1305(key);
        
        var messageLengthWithoutTag = cipher.Length - CryptoConstants.CHACHA20_POLY1305_TAG_LEN;
        var message = cipher[..messageLengthWithoutTag];
        var tag = cipher[messageLengthWithoutTag..];

        chaCha20Poly1305.Decrypt(publicNonce, message, tag, clearTextMessage, authenticationData);

        messageLength = messageLengthWithoutTag;

        return 0;
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
            VirtualUnlock(addr, len);
        }
        // else
        // {
            // TODO: Log somewhere that Memory unlock is not available on this platform.
            // but don't fail so the process can continue
        // }
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