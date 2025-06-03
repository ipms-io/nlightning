using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Node.ValueObjects;

namespace NLightning.Domain.Node.Models;

public class Peer
{
    public CompactPubKey CompactPubKey { get; set; }
    public PeerNodeInfo NodeInfo { get; set; }
    public List<ChannelId> Channels { get; set; } = [];

    public Peer(CompactPubKey compactPubKey, PeerNodeInfo nodeInfo)
    {
        CompactPubKey = compactPubKey;
        NodeInfo = nodeInfo;
    }
}