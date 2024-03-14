using System.Reflection;
using System.Text;
using NLightning.Bolts.BOLT8.Noise.Primitives;
using NLightning.Bolts.BOLT8.Noise.States;
using NLightning.Bolts.Tests.BOLT8.Noise.Mock;

namespace NLightning.Bolts.Tests.BOLT8.Noise.Utils;

internal sealed class InitializedPartiesUtil
{
    public Transport InitiatorTransport { get; }
    public Transport ResponderTransport { get; }
    public byte[] InitiatorSk { get; }
    public byte[] InitiatorRk { get; }

    public InitializedPartiesUtil()
    {
        var _initiatorValidKeys = new InitiatorValidKeysUtil();
        var _responderValidKeys = new ResponderValidKeysUtil();
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _initiatorValidKeys.LocalStaticPrivateKey, _responderValidKeys.LocalStaticPublicKey);
        var responder = protocol.Create(false, _responderValidKeys.LocalStaticPrivateKey, _responderValidKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FakeFixedKeyDh(_initiatorValidKeys.EphemeralPrivateKey)]);
        setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_responderValidKeys.EphemeralPrivateKey)]);

        var initiatorMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        Transport? initiatorTransport;
        Span<byte> initiatorMessage;
        int initiatorMessageSize;

        var responderMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
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
        var c1 = ((CipherState?)InitiatorTransport.GetType().GetField("_sendingKey", flags)?.GetValue(InitiatorTransport) ?? throw new MissingFieldException("_sendingKey")) ?? throw new ArgumentNullException("_sendingKey");
        InitiatorSk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_sendingKey._k")) ?? throw new ArgumentNullException("_sendingKey._k");
        // Get rk
        var c2 = ((CipherState?)InitiatorTransport.GetType().GetField("_receivingKey", flags)?.GetValue(InitiatorTransport) ?? throw new MissingFieldException("_receivingKey")) ?? throw new ArgumentNullException("_receivingKey");
        InitiatorRk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_receivingKey._k")) ?? throw new ArgumentNullException("_receivingKey._k");
    }
}