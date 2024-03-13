using System.Reflection;
using System.Text;
using NLightning.Bolts.BOLT8.Noise.Ciphers;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.Primitives;
using NLightning.Bolts.BOLT8.Noise.States;
using NLightning.Bolts.Tests.BOLT8.Noise.Mock;

namespace NLightning.Bolts.Tests.BOLT8.Noise.IntegrationTests;

public partial class EndToEntIntegrationTests
{
    private static readonly InitiatorValidKeys _initiatorValidKeys = new();
    private static readonly ResponderValidKeys _responderValidKeys = new();

    [Fact]
    public void Given_TwoParties_When_HandshakeEnds_Then_KeysMatch()
    {
        // Arrange
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _initiatorValidKeys.LocalStaticPrivateKey, _responderValidKeys.LocalStaticPublicKey);
        var responder = protocol.Create(false, _responderValidKeys.LocalStaticPrivateKey, _responderValidKeys.LocalStaticPublicKey);

        var initiatorMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? initiatorTransport;
        byte[]? initiatorHandshakeHash;
        Span<byte> initiatorMessage;
        int initiatorMessageSize;

        var responderMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? responderTransport;
        byte[]? responderHandshakeHash;
        Span<byte> responderMessage;
        int responderMessageSize;

        // Act
        // - Initiator writes act one
        (initiatorMessageSize, initiatorHandshakeHash, initiatorTransport) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

        // - Responder reads act one
        (responderMessageSize, responderHandshakeHash, responderTransport) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

        // - Responder writes act two
        (responderMessageSize, responderHandshakeHash, responderTransport) = responder.WriteMessage(responderMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

        // - Initiator reads act two
        (initiatorMessageSize, initiatorHandshakeHash, initiatorTransport) = initiator.ReadMessage(responderMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

        // - Initiator writes act three
        (initiatorMessageSize, initiatorHandshakeHash, initiatorTransport) = initiator.WriteMessage(initiatorMessage.ToArray(), initiatorMessageBuffer);
        initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

        // - Responder reads act three
        (responderMessageSize, responderHandshakeHash, responderTransport) = responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);
        responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

        // Assert
        Assert.NotNull(initiatorHandshakeHash);
        Assert.NotNull(responderHandshakeHash);
        Assert.NotNull(initiatorTransport);
        Assert.NotNull(responderTransport);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        // Get initiator sk
        var c1 = ((CipherState<ChaCha20Poly1305>?)initiatorTransport.GetType().GetField("_c1", flags)?.GetValue(initiatorTransport) ?? throw new MissingFieldException("_c1")) ?? throw new ArgumentNullException("_c1");
        var initiatorSk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_c1._k")) ?? throw new ArgumentNullException("_c1._k");
        // Get initiator rk
        var c2 = ((CipherState<ChaCha20Poly1305>?)initiatorTransport.GetType().GetField("_c2", flags)?.GetValue(initiatorTransport) ?? throw new MissingFieldException("_c2")) ?? throw new ArgumentNullException("_c2");
        var initiatorRk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_c2._k")) ?? throw new ArgumentNullException("_c2._k");
        // Get responder rk
        c1 = ((CipherState<ChaCha20Poly1305>?)responderTransport.GetType().GetField("_c1", flags)?.GetValue(responderTransport) ?? throw new MissingFieldException("_c1")) ?? throw new ArgumentNullException("_c1");
        var responderRk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_c1._k")) ?? throw new ArgumentNullException("_c1._k");
        // Get responder sk
        c2 = ((CipherState<ChaCha20Poly1305>?)responderTransport.GetType().GetField("_c2", flags)?.GetValue(responderTransport) ?? throw new MissingFieldException("_c2")) ?? throw new ArgumentNullException("_c2");
        var responderSk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_c2._k")) ?? throw new ArgumentNullException("_c2._k");

        Assert.Equal(initiatorHandshakeHash, responderHandshakeHash);
        Assert.Equal(initiatorSk, responderRk);
        Assert.Equal(initiatorRk, responderSk);
    }
}