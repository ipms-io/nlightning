namespace NLightning.Domain.Bitcoin.ValueObjects;

public readonly record struct BitcoinSequence(uint Value)
{
    public static implicit operator BitcoinSequence(uint value) => new(value);
    public static implicit operator BitcoinSequence(int value) => new((uint)value);
    public static implicit operator uint(BitcoinSequence lockTime) => lockTime.Value;
}