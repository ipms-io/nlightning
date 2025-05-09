using System.Reflection;
using NLightning.Common.Constants;

namespace NLightning.Bolts.Tests.BOLT8.Utils;

using Bolts.BOLT8.Primitives;
using Bolts.BOLT8.States;
using Common.Crypto.Primitives;
using Mock;

internal sealed class InitializedPartiesUtil
{
    public Transport InitiatorTransport { get; }
    public Transport ResponderTransport { get; }
    public SecureMemory InitiatorSk { get; }
    public SecureMemory InitiatorRk { get; }

    public InitializedPartiesUtil()
    {
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, new FakeFixedKeyDh(InitiatorValidKeysUtil.EphemeralPrivateKey));
        var responder = new HandshakeState(false, ResponderValidKeysUtil.LocalStaticPrivateKey, ResponderValidKeysUtil.LocalStaticPublicKey, new FakeFixedKeyDh(ResponderValidKeysUtil.EphemeralPrivateKey));

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;

        var initiatorMessageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        Transport? initiatorTransport;
        Span<byte> initiatorMessage;
        int initiatorMessageSize;

        var responderMessageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        Transport? responderTransport;
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

        (initiatorMessageSize, _, initiatorTransport) = initiator.WriteMessage(initiatorMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);
        (_, _, responderTransport) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);

        InitiatorTransport = initiatorTransport!;
        ResponderTransport = responderTransport!;

        // Get sk
        var c1 = ((CipherState?)InitiatorTransport.GetType().GetField("_sendingKey", flags)?.GetValue(InitiatorTransport) ?? throw new MissingFieldException("_sendingKey")) ?? throw new NullReferenceException("_sendingKey");
        InitiatorSk = ((SecureMemory?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_sendingKey._k")) ?? throw new NullReferenceException("_sendingKey._k");
        // Get rk
        var c2 = ((CipherState?)InitiatorTransport.GetType().GetField("_receivingKey", flags)?.GetValue(InitiatorTransport) ?? throw new MissingFieldException("_receivingKey")) ?? throw new NullReferenceException("_receivingKey");
        InitiatorRk = ((SecureMemory?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_receivingKey._k")) ?? throw new NullReferenceException("_receivingKey._k");

        initiator.Dispose();
        responder.Dispose();
    }
}