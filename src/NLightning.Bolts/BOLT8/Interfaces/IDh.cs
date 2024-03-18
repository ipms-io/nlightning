namespace NLightning.Bolts.BOLT8.Interfaces;

using Primitives;

/// <summary>
/// Interface for the Diffie-Hellman key exchange.
/// </summary>
internal interface IDh
{
    /// <summary>
    /// Performs a Diffie-Hellman calculation between the private
    /// key in keyPair and the publicKey and writes an output
    /// sequence of bytes of length DhLen into sharedKey parameter.
    /// </summary>
    void Dh(NBitcoin.Key k, ReadOnlySpan<byte> rk, Span<byte> sharedKey);

    /// <summary>
    /// Generates a new Diffie-Hellman key pair.
    /// </summary>
    KeyPair GenerateKeyPair();

    /// <summary>
    /// Generates a Diffie-Hellman key pair from the specified private key.
    /// </summary>
    KeyPair GenerateKeyPair(ReadOnlySpan<byte> privateKey);
}