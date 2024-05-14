namespace NLightning.Bolts.BOLT2.Factories;

using BOLT8.Hashes;

public static class ChannelIdFactory
{
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

        var combined = new byte[66];
        lesserRevocationBasepoint.CopyTo(combined);
        greaterRevocationBasepoint.CopyTo(combined);

        using var hasher = new SHA256();
        hasher.AppendData(combined);
        var hash = new byte[32];
        hasher.GetHashAndReset(hash);
        return new ChannelId(hash);
    }
}