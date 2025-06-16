namespace NLightning.Domain.Channels.Events;

using Crypto.ValueObjects;
using Protocol.Interfaces;

public class ChannelResponseMessageEventArgs : EventArgs
{
    public CompactPubKey PeerPubKey { get; }
    public IChannelMessage ResponseMessage { get; }

    public ChannelResponseMessageEventArgs(CompactPubKey peerPubKey, IChannelMessage responseMessage)
    {
        PeerPubKey = peerPubKey;
        ResponseMessage = responseMessage;
    }
}