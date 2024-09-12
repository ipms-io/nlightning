using System.Runtime.InteropServices;

namespace NLightning.Common.Managers;

using Common.Factories.Crypto;

public static class SecureKeyManager
{
    private static IntPtr s_securePrivateKeyPtr;
    private static ulong s_privateKeyLength;

    public static void Initialize(byte[] privateKey)
    {
        if (s_securePrivateKeyPtr == IntPtr.Zero)
        {
            s_privateKeyLength = (ulong)privateKey.Length;

            using var cryptoProvider = CryptoFactory.GetCryptoProvider();

            // Allocate secure memory
            s_securePrivateKeyPtr = cryptoProvider.MemoryAlloc(s_privateKeyLength);

            // Lock the memory to prevent swapping
            if (cryptoProvider.MemoryLock(s_securePrivateKeyPtr, s_privateKeyLength) == -1)
            {
                throw new InvalidOperationException("Failed to lock memory.");
            }

            // Copy the private key to secure memory
            Marshal.Copy(privateKey, 0, s_securePrivateKeyPtr, (int)s_privateKeyLength);

            // Securely wipe the original key from regular memory
            cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(privateKey, 0), s_privateKeyLength);
        }
        else
        {
            throw new InvalidOperationException("Secure key is already initialized.");
        }
    }

    public static byte[] GetPrivateKey()
    {
        if (s_securePrivateKeyPtr == IntPtr.Zero)
            throw new InvalidOperationException("Secure key is not initialized.");

        var privateKey = new byte[s_privateKeyLength];
        Marshal.Copy(s_securePrivateKeyPtr, privateKey, 0, (int)s_privateKeyLength);

        return privateKey;
    }

    public static void Dispose()
    {
        if (s_securePrivateKeyPtr == IntPtr.Zero)
        {
            return;
        }

        using var cryptoProvider = CryptoFactory.GetCryptoProvider();
        // Securely wipe the memory before freeing it
        cryptoProvider.MemoryZero(s_securePrivateKeyPtr, s_privateKeyLength);

        // Unlock the memory
        cryptoProvider.MemoryUnlock(s_securePrivateKeyPtr, s_privateKeyLength);

        // MemoryFree the memory
        cryptoProvider.MemoryFree(s_securePrivateKeyPtr);
        s_securePrivateKeyPtr = IntPtr.Zero;
    }
}