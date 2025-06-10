namespace NLightning.Domain.Protocol.Interfaces;

using Crypto.ValueObjects;
using Enums;

public interface ISecretStorageService : IDisposable
{
    /// <summary>
    /// Inserts a new secret and verifies it against existing secrets
    /// </summary>
    /// <param name="secret">The secret to insert</param>
    /// <param name="index">The index of the secret</param>
    /// <returns>True if the secret was inserted successfully, false otherwise</returns>
    bool InsertSecret(Secret secret, ulong index);

    /// <summary>
    /// Derives an old secret from a known higher-level secret
    /// </summary>
    /// <param name="index">The index of the secret</param>
    Secret DeriveOldSecret(ulong index);

    /// <summary>
    /// Stores the per-commitment seed securely
    /// </summary>
    /// <param name="secret">The per-commitment secret to store</param>
    void StorePerCommitmentSeed(Secret secret);

    /// <summary>
    /// Retrieves the per-commitment seed
    /// </summary>
    /// <returns>The per-commitment seed as a Secret</returns>
    Secret GetPerCommitmentSeed();

    /// <summary>
    /// Stores the private key for the specified basepoint type securely
    /// </summary>
    /// <param name="type">The type of basepoint associated with the private key</param>
    /// <param name="privKey">The private key to store securely</param>
    void StoreBasepointPrivateKey(BasepointType type, PrivKey privKey);

    /// <summary>
    /// Retrieves the private key associated with the specified basepoint type and key index
    /// </summary>
    /// <param name="keyIndex">The index of the key to retrieve</param>
    /// <param name="type">The basepoint type for which the private key is required</param>
    /// <returns>The private key associated with the specified basepoint type and key index</returns>
    PrivKey GetBasepointPrivateKey(uint keyIndex, BasepointType type);

    /// <summary>
    /// Loads secrets from persistent storage using the specified index
    /// </summary>
    /// <param name="index">The index of the secrets to load</param>
    void LoadFromIndex(uint index);
}