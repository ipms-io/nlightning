using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Factories;

public interface IChannelIdFactory
{
    ChannelId CreateV1(ReadOnlySpan<byte> fundingTxId, ushort fundingOutputIndex);
    ChannelId CreateV2(ReadOnlySpan<byte> lesserRevocationBasepoint, ReadOnlySpan<byte> greaterRevocationBasepoint);
}