using NBitcoin;

namespace NLightning.Common.Crypto.Primitives;

/// <summary>
/// A secp256k1 private/public key pair.
/// </summary>
internal sealed class KeyPair : IDisposable
{
    /// <summary>
    /// Gets the private key.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    public Key PrivateKey { get; }

    /// <summary>
    /// Gets the public key.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    public PubKey PublicKey { get; }

    /// <summary>
    /// Gets the public key bytes.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    public byte[] PublicKeyBytes
    {
        get
        {
            return PublicKey.ToBytes();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyPair"/> class.
    /// </summary>
    /// <param name="privateKey" cref="Key">The private key.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="privateKey"/> is null.
    /// </exception>
    internal KeyPair(Key privateKey)
    {
        PrivateKey = privateKey;
        PublicKey = privateKey.PubKey;
    }

    /// <summary>
    /// Erases the key pair from the memory.
    /// </summary>
    public void Dispose()
    {
        PrivateKey.Dispose();
    }
}