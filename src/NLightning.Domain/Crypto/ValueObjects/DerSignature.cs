using NLightning.Domain.Crypto.Constants;
using NLightning.Domain.Interfaces;

namespace NLightning.Domain.Crypto.ValueObjects;

public record DerSignature : IValueObject
{
    public byte[] Value { get; }
    
    public DerSignature(byte[] value)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        if (value.Length is < CryptoConstants.MinSignatureSize or > CryptoConstants.MaxSignatureSize)
            throw new ArgumentOutOfRangeException(nameof(value), $"Signature must be less than or equal to {CryptoConstants.MaxSignatureSize} bytes");
        
        Value = value;
    }
    
    public static implicit operator DerSignature(byte[] bytes) => new(bytes);
    public static implicit operator byte[](DerSignature hash) => hash.Value;
    
    public static implicit operator ReadOnlyMemory<byte>(DerSignature compactPubKey) => compactPubKey.Value;
}