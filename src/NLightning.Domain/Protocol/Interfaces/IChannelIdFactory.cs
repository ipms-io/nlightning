using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Protocol.Interfaces;

public interface IChannelIdFactory
{
    ChannelId CreateV1(TxId fundingTxId, ushort fundingOutputIndex);
    ChannelId CreateV2(CompactPubKey lesserRevocationBasepoint, CompactPubKey greaterRevocationBasepoint);
}