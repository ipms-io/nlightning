using System.Runtime.InteropServices;

namespace NLightning.Common.Managers;

using Common.Factories.Crypto;

/// <summary>
/// Manages a securely stored private key using protected memory allocation.
/// This class ensures that the private key remains inaccessible from regular memory
/// and is securely wiped when no longer needed.
/// </summary>
public static class SecureKeyManager
{
    private static IntPtr s_securePrivateKeyPtr;
    private static ulong s_privateKeyLength;

    /// <summary>
    /// Initializes the secure key manager with a private key.
    /// </summary>
    /// <param name="privateKey">The private key to store in secure memory.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the key is already initialized or if memory locking fails.
    /// </exception>
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

    /// <summary>
    /// Retrieves the private key stored in secure memory.
    /// </summary>
    /// <returns>The private key as a byte array.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the key is not initialized.</exception>
    public static byte[] GetPrivateKey()
    {
        if (s_securePrivateKeyPtr == IntPtr.Zero)
            throw new InvalidOperationException("Secure key is not initialized.");

        var privateKey = new byte[s_privateKeyLength];
        Marshal.Copy(s_securePrivateKeyPtr, privateKey, 0, (int)s_privateKeyLength);

        return privateKey;
    }

    /// <summary>
    /// Releases the private key from secure memory and wipes its contents.
    /// </summary>
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