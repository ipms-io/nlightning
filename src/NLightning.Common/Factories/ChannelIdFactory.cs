namespace NLightning.Common.Factories;

using Common.Crypto.Hashes;
using Types;

public static class ChannelIdFactory
{
    [Obsolete("Method is deprecated because we've only implemented V2 Channels", true)]
    public static ChannelId CreateV1(Span<byte> fundingTxId, ushort fundingOutputIndex)
    {
        if (fundingTxId.Length != 32)
        {
            throw new ArgumentException("Funding transaction ID must be 32 bytes", nameof(fundingTxId));
        }

        var channelId = new byte[32];
        fundingTxId.CopyTo(channelId);

        // XOR the last 2 bytes with the funding_output_index
        channelId[30] ^= (byte)(fundingOutputIndex >> 8);
        channelId[31] ^= (byte)(fundingOutputIndex & 0xFF);

        return new ChannelId(channelId);
    }

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