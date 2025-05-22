namespace NLightning.Domain.ValueObjects;

using Interfaces;

/// <summary>
/// Represents a chain hash.
/// </summary>
/// <remarks>
/// A chain hash is a 32-byte hash used to identify a chain.
/// </remarks>
public readonly record struct ChainHash : IValueObject
{
    /// <summary>
    /// The length of a chain hash.
    /// </summary>
    public const int LENGTH = 32;

    private readonly byte[] _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="ChainHash"/> struct.
    /// </summary>
    /// <param name="value">The value of the chain hash.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the value is not 32 bytes.</exception>
    public ChainHash(ReadOnlySpan<byte> value)
    {
        if (value.Length != 32)
        {
            throw new ArgumentOutOfRangeException(nameof(value), "ChainHash must be 32 bytes");
        }

        _value = value.ToArray();
    }

    public static implicit operator byte[](ChainHash c) => c._value;
    public static implicit operator ReadOnlyMemory<byte>(ChainHash c) => c._value;
    public static implicit operator ChainHash(byte[] value) => new(value);
}