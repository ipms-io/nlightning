using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Bolts.BOLT3.Services;

using Common.Crypto.Hashes;
using Common.Factories.Crypto;
using Common.Interfaces.Crypto;
using Types;

/// <summary>
/// Provides efficient storage of per-commitment secrets
/// </summary>
public class SecretStorageService : IDisposable
{
    public const int SECRET_SIZE = 32;

    private readonly StoredSecret?[] _knownSecrets = new StoredSecret?[49];
    private readonly ICryptoProvider _cryptoProvider = CryptoFactory.GetCryptoProvider();

    /// <summary>
    /// Inserts a new secret and verifies it against existing secrets
    /// </summary>
    public bool InsertSecret(ReadOnlySpan<byte> secret, ulong index)
    {
        if (secret is not { Length: SECRET_SIZE })
            throw new ArgumentException($"Secret must be {SECRET_SIZE} bytes", nameof(secret));

        // Find bucket for this secret
        var bucket = GetBucketIndex(index);

        var storedSecret = new byte[SECRET_SIZE];
        var derivedSecret = new byte[SECRET_SIZE];
        // Verify this secret can derive all previously known secrets
        for (var b = 0; b < bucket; b++)
        {
            if (_knownSecrets[b] == null)
                continue;

            DeriveSecret(secret, bucket, _knownSecrets[b]!.Index, derivedSecret);

            // Compare with stored secret (copied from secure memory)
            Marshal.Copy(_knownSecrets[b]!.SecretPtr, storedSecret, 0, SECRET_SIZE);

            if (!CryptographicOperations.FixedTimeEquals(derivedSecret, storedSecret))
            {
                // Securely wipe the temporary copy
                _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(storedSecret, 0), SECRET_SIZE);
                _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(derivedSecret, 0), SECRET_SIZE);
                return false; // Secret verification failed
            }

            // Securely wipe the temporary copies
            _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(storedSecret, 0), SECRET_SIZE);
            _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(derivedSecret, 0), SECRET_SIZE);
        }

        if (_knownSecrets[bucket] != null)
        {
            // Free previous secret in this bucket if it exists
            FreeSecret(_knownSecrets[bucket]!.SecretPtr);
        }

        // Allocate secure memory for the new secret
        var securePtr = _cryptoProvider.MemoryAlloc(SECRET_SIZE);

        // Lock memory to prevent swapping
        _cryptoProvider.MemoryLock(securePtr, SECRET_SIZE);

        // Copy secret to secure memory
        Marshal.Copy(secret.ToArray(), 0, securePtr, SECRET_SIZE);

        // Store in the appropriate bucket
        _knownSecrets[bucket] = new StoredSecret(index, securePtr);

        return true;
    }

    /// <summary>
    /// Derives an old secret from a known higher-level secret
    /// </summary>
    public void DeriveOldSecret(ulong index, Span<byte> derivedSecret)
    {
        // Try to find a base secret that can derive this one
        for (var b = 0; b < _knownSecrets.Length; b++)
        {
            if (_knownSecrets[b] == null)
                continue;

            // Check if this secret can derive the requested index
            var mask = ~((1UL << b) - 1);
            if ((index & mask) != (_knownSecrets[b]!.Index & mask))
            {
                continue;
            }

            // Found a base secret that can derive the requested one
            var baseSecret = new byte[SECRET_SIZE];
            Marshal.Copy(_knownSecrets[b]!.SecretPtr, baseSecret, 0, SECRET_SIZE);

            DeriveSecret(baseSecret, b, index, derivedSecret);

            // Securely wipe the temporary base secret
            _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(baseSecret, 0), SECRET_SIZE);

            return; // Success
        }

        throw new InvalidOperationException($"Cannot derive secret for index {index}");
    }

    private static int GetBucketIndex(ulong index)
    {
        for (var b = 0; b < 48; b++)
        {
            if (((index >> b) & 1) == 1)
            {
                return b;
            }
        }
        return 48; // For index 0 (seed)
    }

    private static void DeriveSecret(ReadOnlySpan<byte> baseSecret, int bits, ulong index, Span<byte> derivedSecret)
    {
        using var sha256 = new Sha256();

        baseSecret.CopyTo(derivedSecret);

        for (var b = bits - 1; b >= 0; b--)
        {
            if (((index >> b) & 1) == 0)
            {
                continue;
            }

            derivedSecret[b / 8] ^= (byte)(1 << (b % 8));

            sha256.AppendData(derivedSecret);
            sha256.GetHashAndReset(derivedSecret);
        }
    }

    /// <summary>
    /// Securely frees a secret from memory
    /// </summary>
    private void FreeSecret(IntPtr secretPtr)
    {
        if (secretPtr == IntPtr.Zero)
            return;

        // Wipe memory before freeing
        _cryptoProvider.MemoryZero(secretPtr, SECRET_SIZE);

        // Unlock memory
        _cryptoProvider.MemoryUnlock(secretPtr, SECRET_SIZE);

        // Free memory
        _cryptoProvider.MemoryFree(secretPtr);
    }

    private void ReleaseUnmanagedResources()
    {
        // Free all secrets
        for (var i = 0; i < _knownSecrets.Length; i++)
        {
            if (_knownSecrets[i] == null)
            {
                continue;
            }

            FreeSecret(_knownSecrets[i]!.SecretPtr);
            _knownSecrets[i] = null;
        }
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            _cryptoProvider.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~SecretStorageService()
    {
        Dispose(false);
    }
}