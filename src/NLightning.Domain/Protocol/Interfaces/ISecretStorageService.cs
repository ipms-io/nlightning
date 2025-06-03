using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Protocol.Interfaces;

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
}