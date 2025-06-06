namespace NLightning.Domain.Bitcoin.ValueObjects;

using Domain.Interfaces;
using Utils.Extensions;

public readonly struct Witness : IValueObject, IEquatable<Witness>
{
    private readonly byte[] _value;

    public ushort Length => (ushort)_value.Length;

    public Witness(byte[] value)
    {
        _value = value;
    }

    #region Implicit Operators

    public static implicit operator byte[](Witness s) => s._value;
    public static implicit operator Witness(byte[] value) => new(value);
    public static implicit operator ReadOnlyMemory<byte>(Witness s) => s._value;

    #endregion

    public bool Equals(Witness other)
    {
        return _value.SequenceEqual(other._value);
    }

    public override bool Equals(object? obj)
    {
        return obj is Witness other && Equals(other);
    }

    public override int GetHashCode()
    {
        return _value.GetByteArrayHashCode();
    }
}