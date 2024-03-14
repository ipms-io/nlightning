using System.Reflection;
using System.Text;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Noise.Mock;

internal sealed class InitializedParties
{
    public ITransport InitiatorTransport { get; }
    public ITransport ResponderTransport { get; }

    public byte[] InitiatorHandshakeHash { get; }

    public InitializedParties()
    {
        var _initiatorValidKeys = new InitiatorValidKeys();
        var _responderValidKeys = new ResponderValidKeys();
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _initiatorValidKeys.LocalStaticPrivateKey, _responderValidKeys.LocalStaticPublicKey);
        var responder = protocol.Create(false, _responderValidKeys.LocalStaticPrivateKey, _responderValidKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_initiatorValidKeys.EphemeralPrivateKey)]);
        setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_initiatorValidKeys.EphemeralPrivateKey)]);

        var initiatorMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? initiatorTransport;
        byte[]? initiatorHandshakeHash;
        Span<byte> initiatorMessage;
        int initiatorMessageSize;

        var responderMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? responderTransport;
        Span<byte> responderMessage;
        int responderMessageSize;

        (initiatorMessageSize, _, _) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);
        (responderMessageSize, _, _) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

        (responderMessageSize, _, _) = responder.WriteMessage(responderMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);
        (initiatorMessageSize, _, _) = initiator.ReadMessage(responderMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);


        (initiatorMessageSize, initiatorHandshakeHash, initiatorTransport) = initiator.WriteMessage(initiatorMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);
        (_, _, responderTransport) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);

        InitiatorTransport = initiatorTransport!;
        ResponderTransport = responderTransport!;
        InitiatorHandshakeHash = initiatorHandshakeHash!;
    }
}