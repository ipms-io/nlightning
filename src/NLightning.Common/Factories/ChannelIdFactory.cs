namespace NLightning.Common.Factories;

using Common.Crypto.Hashes;
using Types;

public static class ChannelIdFactory
{
    public static ChannelId CreateV2(Span<byte> lesserRevocationBasepoint, Span<byte> greaterRevocationBasepoint)
    {
        if (lesserRevocationBasepoint.Length != 33 || greaterRevocationBasepoint.Length != 33)
        {
            throw new ArgumentException("Revocation basepoints must be 33 bytes each");
        }

        Span<byte> combined = stackalloc byte[66];
        lesserRevocationBasepoint.CopyTo(combined);
        greaterRevocationBasepoint.CopyTo(combined[33..]);

        using var hasher = new Sha256();
        hasher.AppendData(combined);
        Span<byte> hash = stackalloc byte[32];
        hasher.GetHashAndReset(hash);
        return new ChannelId(hash);
    }
}