using System.Reflection;
using System.Text;

namespace NLightning.Bolts.Tests.BOLT8.IntegrationTests;

using Bolts.BOLT8.States;
using Common.Constants;
using Common.Crypto.Primitives;
using Common.Interfaces.Crypto;
using Mock;
using Tests.Utils;
using Utils;

[Collection(TestCollections.NetworkCollection.NAME)]
public class InitiatorIntegrationTests
{
    private static readonly IEcdh s_dhFake = new FakeFixedKeyDh(InitiatorValidKeysUtil.EphemeralPrivateKey);

    [Fact]
    public void Given_ValidKeys_When_InitiatorWritesActOne_Then_OutputShouldBeValid()
    {
        // Arrange
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey,
                                           InitiatorValidKeysUtil.RemoteStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

        try
        {
            // Act
            var (messageSize, handshakeHash, transport) =
                initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
            var message = messageBuffer[..messageSize];

            // compare bytes actOneOutput to initMessage
            Assert.Equal(InitiatorValidKeysUtil.ActOneOutput, message.ToArray());
            Assert.Null(handshakeHash);
            Assert.Null(transport);
        }
        finally
        {
            initiator.Dispose();
        }
    }

    [Fact]
    public void Given_ValidActTwoMessage_When_InitiatorReadsActTwo_Then_ItDoesntThrow()
    {
        // Arrange
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey,
                                           InitiatorValidKeysUtil.RemoteStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

        try
        {
            // - Play ActOne
            _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

            // Act
            var (messageSize, handshakeHash, transport) =
                initiator.ReadMessage(InitiatorValidKeysUtil.ActTwoInput, messageBuffer);
            var message = messageBuffer[..messageSize];

            // make sure reply is empty
            Assert.Equal([], message.ToArray());
            Assert.Null(handshakeHash);
            Assert.Null(transport);
        }
        finally
        {
            initiator.Dispose();
        }
    }

    [Fact]
    public void Given_ValidActTwoRead_When_InitiatorWritesActThree_Then_OutputShouldBeValid()
    {
        // Arrange
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey,
                                           InitiatorValidKeysUtil.RemoteStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

        try
        {
            // - Play ActOne
            _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
            // - Play ActTwo
            _ = initiator.ReadMessage(InitiatorValidKeysUtil.ActTwoInput, messageBuffer);

            // Act
            var (messageSize, handshakeHash, transport) =
                initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
            var message = messageBuffer[..messageSize];

            // compare bytes actThreeOutput to initMessage
            Assert.Equal(InitiatorValidKeysUtil.ActThreeOutput, message.ToArray());
            Assert.NotNull(handshakeHash);
            Assert.NotNull(transport);
        }
        finally
        {
            initiator.Dispose();
        }
    }

    [Fact]
    public void Given_ValidActTwoRead_When_InitiatorWritesActThree_Then_KeysShouldBeValid()
    {
        // Arrange
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey,
                                           InitiatorValidKeysUtil.RemoteStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

        try
        {
            // - Play ActOne
            _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
            // - Play ActTwo
            _ = initiator.ReadMessage(InitiatorValidKeysUtil.ActTwoInput, messageBuffer);
            // - Play ActThree
            var (_, _, transport) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
            Assert.NotNull(transport);

            // Act
            // Get sk
            var c1 = ((CipherState?)transport.GetType().GetField("_sendingKey", FLAGS)?.GetValue(transport) ??
                      throw new MissingFieldException("_sendingKey")) ??
                     throw new NullReferenceException("_sendingKey");
            var sk = ((SecureMemory?)c1.GetType().GetField("_k", FLAGS)?.GetValue(c1) ??
                      throw new MissingFieldException("_sendingKey._k")) ??
                     throw new NullReferenceException("_sendingKey._k");
            // Get rk
            var c2 = ((CipherState?)transport.GetType().GetField("_receivingKey", FLAGS)?.GetValue(transport) ??
                      throw new MissingFieldException("_receivingKey")) ??
                     throw new NullReferenceException("_receivingKey");
            var rk = ((SecureMemory?)c2.GetType().GetField("_k", FLAGS)?.GetValue(c2) ??
                      throw new MissingFieldException("_receivingKey._k")) ??
                     throw new NullReferenceException("_receivingKey._k");

            Assert.Equal(InitiatorValidKeysUtil.OutputSk, ((Span<byte>)sk).ToArray());
            Assert.Equal(InitiatorValidKeysUtil.OutputRk, ((Span<byte>)rk).ToArray());
        }
        finally
        {
            initiator.Dispose();
        }
    }

    [Theory]
    [InlineData("0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730", "Noise message must be equal to 50 bytes in length.")]
    [InlineData("0102466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae", "Invalid handshake version.")]
    [InlineData("0004466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae", "Invalid public key")]
    [InlineData("0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730af", "Decryption failed.")]
    public void Given_InvalidActTwoMessage_When_InitiatorReadsActTwo_Then_Throws(string actTwoInput,
                                                                                 string exceptionMessage)
    {
        // Arrange
        var initiator = new HandshakeState(true, InitiatorValidKeysUtil.LocalStaticPrivateKey,
                                           InitiatorValidKeysUtil.RemoteStaticPublicKey, s_dhFake);
        var messageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        var inputBytes = actTwoInput.ToByteArray();

        try
        {
            // - Play ActOne
            _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

            // Act
            var exception = Assert.ThrowsAny<Exception>(() => initiator.ReadMessage(inputBytes, messageBuffer));
            Assert.Equal(exceptionMessage, exception.Message);
        }
        finally
        {
            initiator.Dispose();
        }
    }
}