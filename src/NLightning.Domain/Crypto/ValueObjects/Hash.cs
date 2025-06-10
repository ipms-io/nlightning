namespace NLightning.Domain.Crypto.ValueObjects;

using Constants;

public readonly record struct Hash
{
    public byte[] Value { get; }
    public static Hash Empty => new(new byte[CryptoConstants.Sha256HashLen]);

    public Hash(byte[] value)
    {
        if (value.Length < CryptoConstants.Sha256HashLen)
            throw new ArgumentOutOfRangeException(nameof(value), value.Length,
                                                  $"Hash must have {CryptoConstants.Sha256HashLen} bytes.");

        Value = value;
    }

    public static implicit operator Hash(byte[] bytes) => new(bytes);
    public static implicit operator byte[](Hash hash) => hash.Value;

    public static implicit operator ReadOnlyMemory<byte>(Hash hash) => hash.Value;
}