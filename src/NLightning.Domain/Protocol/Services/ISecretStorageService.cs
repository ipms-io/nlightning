namespace NLightning.Domain.Protocol.Services;

public interface ISecretStorageService : IDisposable
{
    /// <summary>
    /// Inserts a new secret and verifies it against existing secrets
    /// </summary>
    /// <param name="secret">The secret to insert</param>
    /// <param name="index">The index of the secret</param>
    /// <returns>True if the secret was inserted successfully, false otherwise</returns>
    bool InsertSecret(ReadOnlySpan<byte> secret, ulong index);

    /// <summary>
    /// Derives an old secret from a known higher-level secret
    /// </summary>
    /// <param name="index">The index of the secret</param>
    /// <param name="derivedSecret">The derived secret</param>
    void DeriveOldSecret(ulong index, Span<byte> derivedSecret);
}