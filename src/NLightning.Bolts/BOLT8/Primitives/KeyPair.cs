using NBitcoin;
using NLightning.Common.Utils;

namespace NLightning.Bolts.BOLT8.Primitives;

using Dhs;

/// <summary>
/// A Diffie-Hellman private/public key pair.
/// </summary>
internal sealed class KeyPair : IDisposable
{
    private static readonly Secp256k1 s_dh = new();
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
            Exceptions.ThrowIfDisposed(_disposed, nameof(KeyPair));
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
            Exceptions.ThrowIfDisposed(_disposed, nameof(KeyPair));
            return _publicKey;
        }
    }

    /// <summary>
    /// Gets the private key bytes.
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// Thrown if the current instance has already been disposed.
    /// </exception>
    public byte[] PrivateKeyBytes
    {
        get
        {
            Exceptions.ThrowIfDisposed(_disposed, nameof(KeyPair));
            return _privateKey.ToBytes();
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
            Exceptions.ThrowIfDisposed(_disposed, nameof(KeyPair));
            if (_publicKey.IsCompressed)
            {
                return _publicKey.ToBytes();
            }
            else
            {
                return _publicKey.Compress().ToBytes();
            }
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
        Exceptions.ThrowIfNull(privateKey, nameof(KeyPair));
        _privateKey = privateKey;
        _publicKey = privateKey.PubKey;
    }

    /// <summary>
    /// Generates a new Diffie-Hellman key pair.
    /// </summary>
    /// <returns>A randomly generated private key and its corresponding public key.</returns>
    public static KeyPair Generate()
    {
        return s_dh.GenerateKeyPair();
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