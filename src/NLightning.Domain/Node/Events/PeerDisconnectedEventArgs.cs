namespace NLightning.Domain.Node.Events;

using Crypto.ValueObjects;

public class PeerDisconnectedEventArgs : EventArgs
{
    public CompactPubKey PeerPubKey { get; }

    public PeerDisconnectedEventArgs(CompactPubKey peerPubKey)
    {
        PeerPubKey = peerPubKey;
    }
}