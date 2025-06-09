namespace NLightning.Domain.Crypto.ValueObjects;

using Constants;
using Utils.Extensions;

public readonly struct Secret : IEquatable<Secret>
{
    private readonly byte[] _value;
    public static Secret Empty => new(new byte[CryptoConstants.SecretLen]);

    public Secret(byte[] value)
    {
        if (value.Length < CryptoConstants.SecretLen)
            throw new ArgumentOutOfRangeException(nameof(value), value.Length,
                                                  $"Hash must have {CryptoConstants.SecretLen} bytes.");

        _value = value;
    }

    public static implicit operator Secret(byte[] bytes) => new(bytes);
    public static implicit operator byte[](Secret hash) => hash._value;

    public static implicit operator ReadOnlyMemory<byte>(Secret hash) => hash._value;
    public static implicit operator ReadOnlySpan<byte>(Secret hash) => hash._value;

    public bool Equals(Secret other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Secret other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetByteArrayHashCode();
    }
}