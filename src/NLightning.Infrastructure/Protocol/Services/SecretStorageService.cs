using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace NLightning.Infrastructure.Protocol.Services;

using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.Enums;
using Domain.Protocol.Interfaces;
using Crypto.Factories;
using Crypto.Hashes;
using Crypto.Interfaces;
using Models;

/// <summary>
/// Provides efficient storage of per-commitment secrets
/// </summary>
public class SecretStorageService : ISecretStorageService
{
    private readonly StoredSecret?[] _knownSecrets = new StoredSecret?[49];
    private readonly ICryptoProvider _cryptoProvider = CryptoFactory.GetCryptoProvider();
    private IntPtr _perCommitmentSeedPtr = IntPtr.Zero;
    private readonly Dictionary<BasepointType, IntPtr> _basepointSecrets = new();

    /// <inheritdoc/>
    public bool InsertSecret(Secret secret, ulong index)
    {
        // Find the bucket for this secret
        var bucket = GetBucketIndex(index);

        var storedSecret = new byte[CryptoConstants.SecretLen];
        var derivedSecret = new byte[CryptoConstants.SecretLen];
        // Verify this secret can derive all previously known secrets
        for (var b = 0; b < bucket; b++)
        {
            if (_knownSecrets[b] == null)
                continue;

            DeriveSecret(secret, bucket, _knownSecrets[b]!.Index, derivedSecret);

            // Compare with stored secret (copied from secure memory)
            Marshal.Copy(_knownSecrets[b]!.SecretPtr, storedSecret, 0, CryptoConstants.SecretLen);

            if (!CryptographicOperations.FixedTimeEquals(derivedSecret, storedSecret))
            {
                // Securely wipe the temporary copy
                _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(storedSecret, 0),
                                           CryptoConstants.SecretLen);
                _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(derivedSecret, 0),
                                           CryptoConstants.SecretLen);
                return false; // Secret verification failed
            }

            // Securely wipe the temporary copies
            _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(storedSecret, 0),
                                       CryptoConstants.SecretLen);
            _cryptoProvider.MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(derivedSecret, 0),
                                       CryptoConstants.SecretLen);
        }

        if (_knownSecrets[bucket] != null)
        {
            // Free previous secret in this bucket if it exists
            FreeSecret(_knownSecrets[bucket]!.SecretPtr);
        }

        // Allocate secure memory for the new secret
        var securePtr = _cryptoProvider.MemoryAlloc(CryptoConstants.SecretLen);

        // Lock memory to prevent swapping
        _cryptoProvider.MemoryLock(securePtr, CryptoConstants.SecretLen);

        // Copy secret to secure memory
        Marshal.Copy(secret, 0, securePtr, CryptoConstants.SecretLen);

        // Store in the appropriate bucket
        _knownSecrets[bucket] = new StoredSecret(index, securePtr);

        return true;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the secret cannot be derived</exception>
    public Secret DeriveOldSecret(ulong index)
    {
        Span<byte> derivedSecret = stackalloc byte[CryptoConstants.SecretLen];
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
            var baseSecret = new byte[CryptoConstants.Sha256HashLen];
            Marshal.Copy(_knownSecrets[b]!.SecretPtr, baseSecret, 0, CryptoConstants.Sha256HashLen);

            DeriveSecret(baseSecret, b, index, derivedSecret);

            // Securely wipe the temporary base secret
            _cryptoProvider
               .MemoryZero(Marshal.UnsafeAddrOfPinnedArrayElement(baseSecret, 0), CryptoConstants.Sha256HashLen);

            return new Secret(derivedSecret.ToArray()); // Success
        }

        throw new InvalidOperationException($"Cannot derive secret for index {index}");
    }

    /// <inheritdoc/>
    public void StorePerCommitmentSeed(Secret secret)
    {
        // Free existing seed if any
        if (_perCommitmentSeedPtr != IntPtr.Zero)
            FreeSecret(_perCommitmentSeedPtr);

        // Allocate secure memory for the seed
        _perCommitmentSeedPtr = _cryptoProvider.MemoryAlloc(CryptoConstants.SecretLen);
        _cryptoProvider.MemoryLock(_perCommitmentSeedPtr, CryptoConstants.SecretLen);
        Marshal.Copy(secret, 0, _perCommitmentSeedPtr, CryptoConstants.SecretLen);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the per-commitment seed is not stored</exception>
    public Secret GetPerCommitmentSeed()
    {
        if (_perCommitmentSeedPtr == IntPtr.Zero)
            throw new InvalidOperationException("Per-commitment seed not stored");

        var seed = new byte[CryptoConstants.SecretLen];
        Marshal.Copy(_perCommitmentSeedPtr, seed, 0, CryptoConstants.SecretLen);
        return seed;
    }

    /// <inheritdoc/>
    public void StoreBasepointPrivateKey(BasepointType type, PrivKey privKey)
    {
        // Free existing key if any
        if (_basepointSecrets.TryGetValue(type, out var existingPtr) && existingPtr != IntPtr.Zero)
            FreeSecret(existingPtr);

        // Allocate secure memory for the private key
        var securePtr = _cryptoProvider.MemoryAlloc(CryptoConstants.SecretLen);
        _cryptoProvider.MemoryLock(securePtr, CryptoConstants.SecretLen);
        Marshal.Copy(privKey, 0, securePtr, CryptoConstants.SecretLen);

        _basepointSecrets[type] = securePtr;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Thrown when the basepoint private key is not stored</exception>
    public PrivKey GetBasepointPrivateKey(uint keyIndex, BasepointType type)
    {
        throw new NotImplementedException("Getting basepoint private keys is not implemented yet.");
    }

    /// <inheritdoc/>
    public void LoadFromIndex(uint index)
    {
        throw new NotImplementedException("Loading from index is not implemented yet.");
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
        _cryptoProvider.MemoryZero(secretPtr, CryptoConstants.Sha256HashLen);

        // Unlock memory
        _cryptoProvider.MemoryUnlock(secretPtr, CryptoConstants.Sha256HashLen);

        // Free memory
        _cryptoProvider.MemoryFree(secretPtr);
    }

    private void ReleaseUnmanagedResources()
    {
        // Free all secrets
        for (var i = 0; i < _knownSecrets.Length; i++)
        {
            if (_knownSecrets[i] == null)
                continue;

            FreeSecret(_knownSecrets[i]!.SecretPtr);
            _knownSecrets[i] = null;
        }

        // Free per-commitment seed
        if (_perCommitmentSeedPtr != IntPtr.Zero)
        {
            FreeSecret(_perCommitmentSeedPtr);
            _perCommitmentSeedPtr = IntPtr.Zero;
        }

        // Free basepoint secrets
        foreach (var kvp in _basepointSecrets)
        {
            if (kvp.Value != IntPtr.Zero)
                FreeSecret(kvp.Value);
        }

        _basepointSecrets.Clear();
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
            _cryptoProvider.Dispose();
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