using NLightning.Domain.Crypto.Constants;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Bitcoin.ValueObjects;

public readonly record struct ExtPrivKey
{
    /// <summary>
    /// The private key value.
    /// </summary>
    public byte[] Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivKey"/> struct.
    /// </summary>
    /// <param name="value">The private key value.</param>
    public ExtPrivKey(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value is null || value.Length != CryptoConstants.ExtPrivkeyLen)
            throw new ArgumentException($"Private key must be {CryptoConstants.ExtPrivkeyLen} bytes long.", nameof(value));
        
        Value = value;
    }

    public static implicit operator ExtPrivKey(byte[] bytes) => new(bytes);
    public static implicit operator byte[](ExtPrivKey script) => script.Value;
}