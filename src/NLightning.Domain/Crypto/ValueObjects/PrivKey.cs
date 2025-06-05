using NLightning.Domain.Crypto.Constants;

namespace NLightning.Domain.Crypto.ValueObjects;

public readonly record struct PrivKey
{
    /// <summary>
    /// The private key value.
    /// </summary>
    public byte[] Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivKey"/> struct.
    /// </summary>
    /// <param name="value">The private key value.</param>
    public PrivKey(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is null || value.Length != CryptoConstants.PrivkeyLen)
            throw new ArgumentException($"Private key must be {CryptoConstants.PrivkeyLen} bytes long.", nameof(value));

        Value = value;
    }

    public static implicit operator PrivKey(byte[] bytes) => new(bytes);
    public static implicit operator byte[](PrivKey hash) => hash.Value;

    public static implicit operator ReadOnlySpan<byte>(PrivKey hash) => hash.Value;
}