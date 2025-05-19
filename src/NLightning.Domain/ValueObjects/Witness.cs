namespace NLightning.Domain.ValueObjects;

using Interfaces;

public readonly struct Witness : IValueObject, IEquatable<Witness>
{
    private readonly byte[] _value;

    public ushort Length => (ushort)_value.Length;

    public Witness(byte[] value)
    {
        _value = value;
    }

    #region Equality
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
        return _value.GetHashCode();
    }
    #endregion

    #region Implicit Operators
    public static implicit operator byte[](Witness s) => s._value;
    public static implicit operator Witness(byte[] value) => new(value);
    public static implicit operator ReadOnlyMemory<byte>(Witness s) => s._value;

    public static bool operator ==(Witness left, Witness right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Witness left, Witness right)
    {
        return !(left == right);
    }
    #endregion
}