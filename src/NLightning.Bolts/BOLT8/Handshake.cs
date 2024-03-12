using NBitcoin;

namespace NLightning.Bolts.BOLT8;

internal sealed class Handshake
{
    private readonly HandshakeState _handshakeState;
    private readonly PubKey _localStaticKey;
    private readonly PubKey _remoteStaticKey;

    public Handshake(HandshakeState handshakeState, PubKey localStaticKey, PubKey remoteStaticKey)
    {
        _handshakeState = handshakeState;
        _localStaticKey = localStaticKey;
        _remoteStaticKey = remoteStaticKey;
    }

    public async Task HandshakeAsync(Stream stream, CancellationToken token)
    {
        // ...
    }
}