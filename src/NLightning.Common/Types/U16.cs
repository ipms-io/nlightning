namespace NLightning.Common;

public readonly struct U16(ushort value)
{
    private readonly ushort _value = value;

    public static implicit operator ushort(U16 u) => u._value;
    public static implicit operator U16(ushort value) => new(value);
}
