namespace NLightning.Domain.Node.Models;

using Channels.ValueObjects;
using Crypto.ValueObjects;
using ValueObjects;

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