using NBitcoin;

namespace NLightning.Bolts.BOLT8.Primitives;

using Common.Utils;

/// <summary>
/// A Diffie-Hellman private/public key pair.
/// </summary>
internal sealed class KeyPair : IDisposable
{
    private readonly Key _privateKey;
    private readonly PubKey _publicKey;
    private bool _disposed;

    /// <summary>
    /// Gets the private key.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    public Key PrivateKey
    {
        get
        {
            ExceptionUtils.ThrowIfDisposed(_disposed, nameof(KeyPair));
            return _privateKey;
        }
    }

    /// <summary>
    /// Gets the public key.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    public PubKey PublicKey
    {
        get
        {
            ExceptionUtils.ThrowIfDisposed(_disposed, nameof(KeyPair));
            return _publicKey;
        }
    }

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
            ExceptionUtils.ThrowIfDisposed(_disposed, nameof(KeyPair));
            return _publicKey.ToBytes();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyPair"/> class.
    /// </summary>
    /// <param name="privateKey" cref="Key">The private key.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the <paramref name="keyPair"/> is null.
    /// </exception>
    internal KeyPair(Key privateKey)
    {
        ExceptionUtils.ThrowIfNull(privateKey, nameof(KeyPair));
        _privateKey = privateKey;
        _publicKey = privateKey.PubKey;
    }

    /// <summary>
    /// Erases the key pair from the memory.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
        }
    }
}