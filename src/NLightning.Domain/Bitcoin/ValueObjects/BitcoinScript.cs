using NLightning.Domain.Interfaces;

namespace NLightning.Domain.Bitcoin.ValueObjects;

public readonly record struct BitcoinScript : IValueObject
{
    public static BitcoinScript Empty => new([]);

    public byte[] Value { get; }

    public BitcoinScript() : this([])
    {
    }

    public BitcoinScript(byte[] value)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value), "BitcoinScript cannot be null.");

        Value = value;
    }

    public static implicit operator BitcoinScript(byte[] bytes) => new(bytes);
    public static implicit operator byte[](BitcoinScript script) => script.Value;
    public static implicit operator ReadOnlyMemory<byte>(BitcoinScript compactPubKey) => compactPubKey.Value;
}