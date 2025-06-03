namespace NLightning.Infrastructure.Crypto.Interfaces;

using Domain.Crypto.ValueObjects;

/// <summary>
/// Interface for the Eliptic-Curve Diffie-Hellman key exchange.
/// </summary>
public interface IEcdh
{
    /// <summary>
    /// Performs a Diffie-Hellman calculation between a secp256k1 private key in keyPair and the publicKey and writes an
    /// output sequence of bytes of length DhLen into sharedKey parameter.
    /// </summary>
    void SecP256K1Dh(PrivKey k, ReadOnlySpan<byte> rk, Span<byte> sharedKey);

    /// <summary>
    /// Generates a new Diffie-Hellman key pair.
    /// </summary>
    CryptoKeyPair GenerateKeyPair();

    /// <summary>
    /// Generates a Diffie-Hellman key pair from the specified private key.
    /// </summary>
    CryptoKeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey);
}