using NLightning.Domain.Crypto.Constants;

namespace NLightning.Domain.Crypto.ValueObjects;

public readonly record struct Secret
{
    public byte[] Value { get; }
    public static Secret Empty => new(new byte[CryptoConstants.SecretLen]);

    public Secret(byte[] value)
    {
        if (value.Length < CryptoConstants.SecretLen)
            throw new ArgumentOutOfRangeException(nameof(value), value.Length,
                $"Hash must have {CryptoConstants.SecretLen} bytes.");

        Value = value;
    }

    public static implicit operator Secret(byte[] bytes) => new(bytes);
    public static implicit operator byte[](Secret hash) => hash.Value;
    
    public static implicit operator ReadOnlyMemory<byte>(Secret hash) => hash.Value;
    public static implicit operator ReadOnlySpan<byte>(Secret hash) => hash.Value;
}