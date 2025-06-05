using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;

namespace NLightning.Infrastructure.Protocol.Factories;

using Crypto.Hashes;

public class ChannelIdFactory : IChannelIdFactory
{
    public ChannelId CreateV1(TxId fundingTxId, ushort fundingOutputIndex)
    {
        Span<byte> channelId = stackalloc byte[32];
        fundingTxId.Hash.CopyTo(channelId);

        // XOR the last 2 bytes with the funding_output_index
        channelId[30] ^= (byte)(fundingOutputIndex >> 8);
        channelId[31] ^= (byte)(fundingOutputIndex & 0xFF);

        return new ChannelId(channelId);
    }

    public ChannelId CreateV2(CompactPubKey lesserRevocationBasepoint, CompactPubKey greaterRevocationBasepoint)
    {
        Span<byte> combined = stackalloc byte[66];
        lesserRevocationBasepoint.CompactBytes.CopyTo(combined);
        greaterRevocationBasepoint.CompactBytes.CopyTo(combined[33..]);

        using var hasher = new Sha256();
        hasher.AppendData(combined);
        Span<byte> hash = stackalloc byte[32];
        hasher.GetHashAndReset(hash);
        return new ChannelId(hash);
    }
}