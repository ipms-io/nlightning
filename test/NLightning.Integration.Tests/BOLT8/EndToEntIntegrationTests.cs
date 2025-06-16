using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NLightning.Tests.Utils.Mocks;
using NLightning.Tests.Utils.Vectors;

namespace NLightning.Integration.Tests.BOLT8;

using Infrastructure.Crypto.Primitives;
using Infrastructure.Protocol.Constants;
using Infrastructure.Transport.Handshake.States;
using Vectors;

public class EndToEntIntegrationTests
{
    [Fact]
    public void Given_TwoParties_When_HandshakeEnds_Then_KeysMatch()
    {
        // Given
        var initiator = new HandshakeState(true, InitiatorValidKeysVector.LocalStaticPrivateKey,
                                           InitiatorValidKeysVector.RemoteStaticPublicKey,
                                           new FakeFixedKeyDh(InitiatorValidKeysVector.LocalStaticPrivateKey));
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey,
                                           new FakeFixedKeyDh(ResponderValidKeysVector.LocalStaticPrivateKey));
        var initiatorMessageBuffer = new byte[ProtocolConstants.MaxMessageLength];
        var responderMessageBuffer = new byte[ProtocolConstants.MaxMessageLength];

        try
        {
            // When
            // - Initiator writes act one
            var (initiatorMessageSize, _, _) =
                initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), initiatorMessageBuffer);
            var initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

            // - Responder reads act one
            var (responderMessageSize, _, _) =
                responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);
            var responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

            // - Responder writes act two
            (responderMessageSize, _, _) = responder.WriteMessage(responderMessage.ToArray(), responderMessageBuffer);
            responderMessage = responderMessageBuffer.AsSpan(0, responderMessageSize);

            // - Initiator reads act two
            (initiatorMessageSize, _, _) = initiator.ReadMessage(responderMessage.ToArray(), initiatorMessageBuffer);
            initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

            // - Initiator writes act three
            (initiatorMessageSize, var initiatorHandshakeHash, var initiatorTransport) =
                initiator.WriteMessage(initiatorMessage.ToArray(), initiatorMessageBuffer);
            initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

            // - Responder reads act three
            var (_, responderHandshakeHash, responderTransport) =
                responder.ReadMessage(initiatorMessage.ToArray(), responderMessageBuffer);

            // Then
            Assert.NotNull(initiatorHandshakeHash);
            Assert.NotNull(responderHandshakeHash);
            Assert.NotNull(initiatorTransport);
            Assert.NotNull(responderTransport);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            // Get initiator sk
            var c1 =
                ((CipherState?)initiatorTransport.GetType().GetField("_sendingKey", flags)
                                                ?.GetValue(initiatorTransport) ??
                 throw new MissingFieldException("_sendingKey")) ??
                throw new NullReferenceException("_sendingKey");
            var initiatorSk =
                ((SecureMemory?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ??
                 throw new MissingFieldException("_sendingKey._k")) ??
                throw new NullReferenceException("_sendingKey._k");
            // Get initiator rk
            var c2 =
                ((CipherState?)initiatorTransport.GetType().GetField("_receivingKey", flags)
                                                ?.GetValue(initiatorTransport) ??
                 throw new MissingFieldException("_receivingKey")) ??
                throw new NullReferenceException("_receivingKey");
            var initiatorRk =
                ((SecureMemory?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ??
                 throw new MissingFieldException("_receivingKey._k")) ??
                throw new NullReferenceException("_receivingKey._k");
            // Get responder sk
            c1 = ((CipherState?)responderTransport.GetType().GetField("_sendingKey", flags)
                                                 ?.GetValue(responderTransport) ??
                  throw new MissingFieldException("_sendingKey")) ??
                 throw new NullReferenceException("_sendingKey");
            var responderSk =
                ((SecureMemory?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ??
                 throw new MissingFieldException("_sendingKey._k")) ??
                throw new NullReferenceException("_sendingKey._k");

            Assert.Equal(initiatorHandshakeHash, responderHandshakeHash);
            Assert.Equal(((Span<byte>)initiatorSk).ToArray(), ((Span<byte>)responderSk).ToArray());
            Assert.Equal(((Span<byte>)initiatorRk).ToArray(), ((Span<byte>)initiatorRk).ToArray());
        }
        finally
        {
            initiator.Dispose();
            responder.Dispose();
        }
    }

    [Fact]
    public void Given_HandshakeBetweenParties_When_MessageIsCorrupted_Then_ExceptionIsThrown()
    {
        // Given
        var initiator = new HandshakeState(true, InitiatorValidKeysVector.LocalStaticPrivateKey,
                                           InitiatorValidKeysVector.RemoteStaticPublicKey,
                                           new FakeFixedKeyDh(InitiatorValidKeysVector.LocalStaticPrivateKey));
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey,
                                           new FakeFixedKeyDh(ResponderValidKeysVector.LocalStaticPrivateKey));
        var initiatorMessageBuffer = new byte[ProtocolConstants.MaxMessageLength];
        var responderMessageBuffer = new byte[ProtocolConstants.MaxMessageLength];

        try
        {
            // When - Initiator writes act one
            var (initiatorMessageSize, _, _) =
                initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), initiatorMessageBuffer);
            var initiatorMessage = initiatorMessageBuffer.AsSpan(0, initiatorMessageSize);

            // Corrupt the message by flipping a bit in the middle
            var corruptedMessage = initiatorMessage.ToArray();
            corruptedMessage[corruptedMessage.Length / 2] ^= 0x01;

            // Then - Responder should throw an exception when reading the corrupted message
            var exception =
                Assert.Throws<CryptographicException>(() => responder.ReadMessage(
                                                          corruptedMessage, responderMessageBuffer));

            Assert.Contains("Decryption failed", exception.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            initiator.Dispose();
            responder.Dispose();
        }
    }
}