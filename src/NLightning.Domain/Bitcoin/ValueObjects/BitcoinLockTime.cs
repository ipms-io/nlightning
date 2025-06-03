namespace NLightning.Domain.Bitcoin.ValueObjects;

public readonly record struct BitcoinLockTime(uint ValueOrHeight)
{
    public static implicit operator BitcoinLockTime(uint value) => new(value);
    public static implicit operator uint(BitcoinLockTime bitcoinLockTime) => bitcoinLockTime.ValueOrHeight;
}