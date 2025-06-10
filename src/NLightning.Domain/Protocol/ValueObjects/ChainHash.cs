namespace NLightning.Domain.Protocol.ValueObjects;

using Crypto.Constants;
using Domain.Interfaces;

/// <summary>
/// Represents a chain hash.
/// </summary>
/// <remarks>
/// A chain hash is a 32-byte hash used to identify a chain.
/// </remarks>
public readonly struct ChainHash : IEquatable<ChainHash>, IValueObject
{
    public byte[] Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainHash"/> struct.
    /// </summary>
    /// <param name="value">The value of the chain hash.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not 32 bytes.</exception>
    public ChainHash(ReadOnlySpan<byte> value)
    {
        if (value.Length != CryptoConstants.Sha256HashLen)
        {
            throw new ArgumentOutOfRangeException(nameof(value),
                                                  $"ChainHash must be {CryptoConstants.Sha256HashLen} bytes");
        }

        Value = value.ToArray();
    }

    public static implicit operator byte[](ChainHash c) => c.Value;
    public static implicit operator ReadOnlyMemory<byte>(ChainHash c) => c.Value;
    public static implicit operator ChainHash(byte[] value) => new(value);

    public static bool operator !=(ChainHash left, ChainHash right)
    {
        return !left.Equals(right);
    }

    public static bool operator ==(ChainHash left, ChainHash right)
    {
        return left.Equals(right);
    }

    public bool Equals(ChainHash other)
    {
        return Value.SequenceEqual(other.Value);
    }

    public override bool Equals(object? obj)
    {
        return obj is ChainHash other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }
}