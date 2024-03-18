using System.Reflection;
using System.Text;
using NLightning.Bolts.BOLT8.Constants;
using NLightning.Bolts.BOLT8.Primitives;
using NLightning.Bolts.BOLT8.States;
using NLightning.Bolts.Tests.BOLT8.Utils;

namespace NLightning.Bolts.Tests.BOLT8.IntegrationTests;

public partial class EndToEntIntegrationTests
{
    private static readonly ResponderValidKeysUtil _responderValidKeys = new();

    [Fact]
    public void Given_TwoParties_When_HandshakeEnds_Then_KeysMatch()
    {
        // Arrange
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey);
        var responder = new HandshakeState(false, ResponderValidKeysUtil.LocalStaticPrivateKey, ResponderValidKeysUtil.LocalStaticPublicKey);

        var initiatorMessageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        Transport? initiatorTransport;
        byte[]? initiatorHandshakeHash;
        Span<byte> initiatorMessage;
        int initiatorMessageSize;

        var responderMessageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        Transport? responderTransport;
        byte[]? responderHandshakeHash;
        Span<byte> responderMessage;
        int responderMessageSize;

        // Act
        // - Initiator writes act one
        (initiatorMessageSize, _, _) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

        // - Responder reads act one
        (responderMessageSize, _, _) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

        // - Responder writes act two
        (responderMessageSize, _, _) = responder.WriteMessage(responderMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

        // - Initiator reads act two
        (initiatorMessageSize, _, _) = initiator.ReadMessage(responderMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

        // - Initiator writes act three
        (initiatorMessageSize, initiatorHandshakeHash, initiatorTransport) = initiator.WriteMessage(initiatorMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

        // - Responder reads act three
        (_, responderHandshakeHash, responderTransport) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);

        // Assert
        Assert.NotNull(initiatorHandshakeHash);
        Assert.NotNull(responderHandshakeHash);
        Assert.NotNull(initiatorTransport);
        Assert.NotNull(responderTransport);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        // Get initiator sk
        var c1 = ((CipherState?)initiatorTransport.GetType().GetField("_sendingKey", flags)?.GetValue(initiatorTransport) ?? throw new MissingFieldException("_sendingKey")) ?? throw new ArgumentNullException("_sendingKey");
        var initiatorSk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_sendingKey._k")) ?? throw new ArgumentNullException("_sendingKey._k");
        // Get initiator rk
        var c2 = ((CipherState?)initiatorTransport.GetType().GetField("_receivingKey", flags)?.GetValue(initiatorTransport) ?? throw new MissingFieldException("_receivingKey")) ?? throw new ArgumentNullException("_receivingKey");
        var initiatorRk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_receivingKey._k")) ?? throw new ArgumentNullException("_receivingKey._k");
        // Get responder sk
        c1 = ((CipherState?)responderTransport.GetType().GetField("_sendingKey", flags)?.GetValue(responderTransport) ?? throw new MissingFieldException("_sendingKey")) ?? throw new ArgumentNullException("_sendingKey");
        var responderSk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_sendingKey._k")) ?? throw new ArgumentNullException("_sendingKey._k");
        // Get responder rk
        c2 = ((CipherState?)responderTransport.GetType().GetField("_receivingKey", flags)?.GetValue(responderTransport) ?? throw new MissingFieldException("_receivingKey")) ?? throw new ArgumentNullException("_receivingKey");
        var responderRk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_receivingKey._k")) ?? throw new ArgumentNullException("_receivingKey._k");

        Assert.Equal(initiatorHandshakeHash, responderHandshakeHash);
        Assert.Equal(initiatorSk, responderSk);
        Assert.Equal(responderRk, initiatorRk);
    }
}