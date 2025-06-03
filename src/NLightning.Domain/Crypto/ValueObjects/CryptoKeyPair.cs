namespace NLightning.Domain.Crypto.ValueObjects;

/// <summary>
/// A secp256k1 private/public key pair.
/// </summary>
public readonly record struct CryptoKeyPair
{
    /// <summary>
    /// Gets the private key.
    /// </summary>
    public PrivKey PrivKey { get; }

    /// <summary>
    /// Gets the public key.
    /// </summary>
    public CompactPubKey CompactPubKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CryptoKeyPair"/> class.
    /// </summary>
    /// <param name="privKey" cref="ValueObjects.PrivKey">The private key.</param>
    /// <param name="compactPubKey" cref="ValueObjects.CompactPubKey"></param>
    public CryptoKeyPair(PrivKey privKey, CompactPubKey compactPubKey)
    {
        PrivKey = privKey;
        CompactPubKey = compactPubKey;
    }
}