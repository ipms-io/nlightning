namespace NLightning.Domain.Node.Events;

using Crypto.ValueObjects;
using Protocol.Interfaces;

public class ChannelMessageEventArgs : EventArgs
{
    public IChannelMessage Message { get; }
    public CompactPubKey PeerPubKey { get; }

    public ChannelMessageEventArgs(IChannelMessage message, CompactPubKey peerPubKey)
    {
        Message = message;
        PeerPubKey = peerPubKey;
    }
}