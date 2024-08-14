using System.Runtime.InteropServices;

namespace NLightning.Common.Managers;

public static class SecureKeyManager
{
    private static IntPtr s_securePrivateKeyPtr;
    private static ulong s_privateKeyLength;

    public static void Initialize(byte[] privateKey)
    {
        if (s_securePrivateKeyPtr == IntPtr.Zero)
        {
            s_privateKeyLength = (ulong)privateKey.Length;

            // Allocate secure memory
            s_securePrivateKeyPtr = Libsodium.sodium_malloc(s_privateKeyLength);

            // Lock the memory to prevent swapping
            Libsodium.sodium_mlock(s_securePrivateKeyPtr, s_privateKeyLength);

            // Copy the private key to secure memory
            Marshal.Copy(privateKey, 0, s_securePrivateKeyPtr, (int)s_privateKeyLength);

            // Securely wipe the original key from regular memory
            Libsodium.sodium_memzero(Marshal.UnsafeAddrOfPinnedArrayElement(privateKey, 0), s_privateKeyLength);
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

        // Securely wipe the memory before freeing it
        Libsodium.sodium_memzero(s_securePrivateKeyPtr, s_privateKeyLength);

        // Unlock the memory
        Libsodium.sodium_munlock(s_securePrivateKeyPtr, s_privateKeyLength);

        // Free the memory
        Libsodium.sodium_free(s_securePrivateKeyPtr);
        s_securePrivateKeyPtr = IntPtr.Zero;
    }
}