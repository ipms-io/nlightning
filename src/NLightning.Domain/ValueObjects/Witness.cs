namespace NLightning.Domain.ValueObjects;

using Interfaces;

public readonly record struct Witness : IValueObject
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
}