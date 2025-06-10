namespace NLightning.Domain.Crypto.ValueObjects;

using Constants;
using Interfaces;

public record CompactSignature : IValueObject
{
    public byte[] Value { get; }

    public CompactSignature(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        if (value.Length is < CryptoConstants.MinSignatureSize or > CryptoConstants.MaxSignatureSize)
            throw new ArgumentOutOfRangeException(nameof(value),
                                                  $"Signature must be less than or equal to {CryptoConstants.MaxSignatureSize} bytes");

        Value = value;
    }

    public static implicit operator CompactSignature(byte[] bytes) => new(bytes);
    public static implicit operator byte[](CompactSignature hash) => hash.Value;

    public static implicit operator ReadOnlyMemory<byte>(CompactSignature compactPubKey) => compactPubKey.Value;
}