using NBitcoin;
using NBitcoin.Secp256k1;

namespace NLightning.Infrastructure.Bitcoin.Services;

using Crypto.Contexts;
using Domain.Crypto.Constants;
using Domain.Crypto.ValueObjects;
using Domain.Protocol.Interfaces;
using Infrastructure.Crypto.Hashes;

public class KeyDerivationService : IKeyDerivationService
{
    /// <summary>
    /// Derives a public key using the formula: basepoint + SHA256(per_commitment_point || basepoint) * G
    /// </summary>
    public CompactPubKey DerivePublicKey(CompactPubKey compactBasepoint, CompactPubKey compactPerCommitmentPoint)
    {
        var basePoint = new PubKey(compactBasepoint);
        var percCommitmentPoint = new PubKey(compactPerCommitmentPoint);

        // Calculate SHA256(per_commitment_point || basepoint)
        Span<byte> hashBytes = stackalloc byte[CryptoConstants.Sha256HashLen];
        ComputeSha256(percCommitmentPoint, basePoint, hashBytes);

        // Create a private key from the hash (this represents the scalar value)
        var hashPrivateKey = new Key(hashBytes.ToArray());

        // Get the EC point representation of hash*G
        var hashPoint = hashPrivateKey.PubKey;

        // Add the base point to the hash point (EC point addition)
        // NBitcoin doesn't have direct point addition, so we use a trick with BIP32 derivation
        return AddPubKeys(basePoint, hashPoint);
    }

    /// <summary>
    /// Derives a private key using the formula: basepoint_secret + SHA256(per_commitment_point || basepoint)
    /// </summary>
    public PrivKey DerivePrivateKey(PrivKey basepointSecretPriv, CompactPubKey compactPerCommitmentPoint)
    {
        var basepointSecret = new Key(basepointSecretPriv);
        var perCommitmentPoint = new PubKey(compactPerCommitmentPoint);

        Span<byte> hashBytes = stackalloc byte[CryptoConstants.Sha256HashLen];
        ComputeSha256(perCommitmentPoint, basepointSecret.PubKey, hashBytes);

        // Create a private key from the hash
        var hashPrivateKey = new Key(hashBytes.ToArray());

        // Combine the two private keys
        return AddPrivateKeys(basepointSecret, hashPrivateKey).ToBytes();
    }

    /// <summary>
    /// Derives the revocation public key
    /// </summary>
    public CompactPubKey DeriveRevocationPubKey(CompactPubKey compactRevocationBasepoint,
                                                CompactPubKey compactPerCommitmentPoint)
    {
        var revocationBasepoint = new PubKey(compactRevocationBasepoint);
        var perCommitmentPoint = new PubKey(compactPerCommitmentPoint);

        Span<byte> hash1 = stackalloc byte[CryptoConstants.Sha256HashLen];
        Span<byte> hash2 = stackalloc byte[CryptoConstants.Sha256HashLen];
        ComputeSha256(revocationBasepoint, perCommitmentPoint, hash1);
        ComputeSha256(perCommitmentPoint, revocationBasepoint, hash2);

        // Calculate revocation_basepoint * SHA256(revocation_basepoint || per_commitment_point)
        var term1PrivKey = new Key(hash1.ToArray());
        var term1 = MultiplyPubKey(revocationBasepoint, term1PrivKey.ToBytes());

        // Calculate per_commitment_point * SHA256(per_commitment_point || revocation_basepoint)
        var term2PrivKey = new Key(hash2.ToArray());
        var term2 = MultiplyPubKey(perCommitmentPoint, term2PrivKey.ToBytes());

        // Add the two terms
        return AddPubKeys(term1, term2);
    }

    /// <summary>
    /// Derives the revocation private key when both secrets are known
    /// </summary>
    public PrivKey DeriveRevocationPrivKey(PrivKey revocationBasepointSecretPriv, PrivKey perCommitmentSecretPriv)
    {
        var revocationBasepointSecret = new Key(revocationBasepointSecretPriv);
        var perCommitmentSecret = new Key(perCommitmentSecretPriv);

        var revocationBasepoint = revocationBasepointSecret.PubKey;
        var perCommitmentPoint = perCommitmentSecret.PubKey;

        Span<byte> hash1 = stackalloc byte[CryptoConstants.Sha256HashLen];
        Span<byte> hash2 = stackalloc byte[CryptoConstants.Sha256HashLen];
        ComputeSha256(revocationBasepoint, perCommitmentPoint, hash1);
        ComputeSha256(perCommitmentPoint, revocationBasepoint, hash2);

        // Calculate revocation_basepoint_secret * SHA256(revocation_basepoint || per_commitment_point)
        var term1 = MultiplyPrivateKey(revocationBasepointSecret, hash1.ToArray());

        // Calculate per_commitment_secret * SHA256(per_commitment_point || revocation_basepoint)
        var term2 = MultiplyPrivateKey(perCommitmentSecret, hash2.ToArray());

        // Add the two terms
        return AddPrivateKeys(term1, term2).ToBytes();
    }

    /// <summary>
    /// Generates per-commitment secret from seed and index
    /// </summary>
    public Secret GeneratePerCommitmentSecret(Secret seed, ulong index)
    {
        using var sha256 = new Sha256();

        var secret = new byte[CryptoConstants.Sha256HashLen];
        Buffer.BlockCopy(seed, 0, secret, 0, CryptoConstants.Sha256HashLen);

        for (var b = 47; b >= 0; b--)
        {
            if (((index >> b) & 1) == 0)
            {
                continue;
            }

            // Flip bit (b % 8) in byte (b / 8)
            secret[b / 8] ^= (byte)(1 << (b % 8));
            sha256.AppendData(secret);
            sha256.GetHashAndReset(secret);
        }

        return secret;
    }

    /// <summary>
    /// Helper method to calculate SHA256(point1 || point2)
    /// </summary>
    private static void ComputeSha256(PubKey point1, PubKey point2, Span<byte> buffer)
    {
        using var sha256 = new Sha256();
        sha256.AppendData(point1.ToBytes());
        sha256.AppendData(point2.ToBytes());
        sha256.GetHashAndReset(buffer);
    }

    /// <summary>
    /// Adds two public keys (EC point addition)
    /// </summary>
    private static CompactPubKey AddPubKeys(PubKey pubKey1, PubKey pubKey2)
    {
        // Create ECPubKey objects
        if (!ECPubKey.TryCreate(pubKey1.ToBytes(), NLightningCryptoContext.Instance, out _, out var ecPubKey1))
            throw new ArgumentException("Invalid public key", nameof(pubKey1));

        if (!ECPubKey.TryCreate(pubKey2.ToBytes(), NLightningCryptoContext.Instance, out _, out var ecPubKey2))
            throw new ArgumentException("Invalid public key", nameof(pubKey2));

        // Use TryCombine to add the pubkeys
        if (!ECPubKey.TryCombine(NLightningCryptoContext.Instance, [ecPubKey1, ecPubKey2], out var combinedPubKey))
            throw new InvalidOperationException("Failed to combine public keys");

        // Create a new PubKey from the combined ECPubKey
        return new PubKey(combinedPubKey!.ToBytes()).ToBytes();
    }

    /// <summary>
    /// Multiplies a public key by a scalar
    /// </summary>
    private static PubKey MultiplyPubKey(PubKey pubKey, byte[] scalar)
    {
        ArgumentNullException.ThrowIfNull(pubKey);
        if (scalar is not { Length: 32 })
            throw new ArgumentException("Scalar must be 32 bytes", nameof(scalar));

        // Convert PubKey to ECPubKey
        if (!ECPubKey.TryCreate(pubKey.ToBytes(), NLightningCryptoContext.Instance, out var compressed, out var ecPubKey))
            throw new ArgumentException("Invalid public key", nameof(pubKey));

        // Multiply using TweakMul
        var multipliedPubKey = ecPubKey.TweakMul(scalar);

        // Create a new PubKey from the result
        return new PubKey(multipliedPubKey.ToBytes(compressed));
    }

    /// <summary>
    /// Adds two private keys (modular addition in the EC field)
    /// </summary>
    private Key AddPrivateKeys(Key key1, Key key2)
    {
        ArgumentNullException.ThrowIfNull(key1);
        ArgumentNullException.ThrowIfNull(key2);

        // Extract the bytes from the second key
        var key2Bytes = key2.ToBytes();

        // Create a temporary ECPrivKey from the first key's bytes
        if (!NLightningCryptoContext.Instance.TryCreateECPrivKey(key1.ToBytes(), out var ecKey1))
            throw new InvalidOperationException("Invalid first private key");

        // Add the second key to the first using TweakAdd
        var resultKey = ecKey1.TweakAdd(key2Bytes);

        // Create a new Key with the result
        return new Key(resultKey.sec.ToBytes());
    }

    /// <summary>
    /// Multiplies a private key by a scalar
    /// </summary>
    private static Key MultiplyPrivateKey(Key key, byte[] scalar)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (scalar is not { Length: 32 })
            throw new ArgumentException("Scalar must be 32 bytes", nameof(scalar));

        // Create a temporary ECPrivKey from the key's bytes
        if (!NLightningCryptoContext.Instance.TryCreateECPrivKey(key.ToBytes(), out var ecKey))
            throw new InvalidOperationException("Invalid private key");

        // Multiply using TweakMul
        var multipliedKey = ecKey.TweakMul(scalar);

        // Create a new Key with the result
        return new Key(multipliedKey.sec.ToBytes());
    }
}