using System.Reflection;
using System.Text;
using NLightning.Infrastructure.Crypto.Interfaces;
using NLightning.Infrastructure.Crypto.Primitives;
using NLightning.Infrastructure.Protocol.Constants;
using NLightning.Infrastructure.Transport.Handshake.States;
using NLightning.Integration.Tests.TestCollections;
using NLightning.Tests.Utils.Mocks;

namespace NLightning.Integration.Tests.BOLT8;

using Vectors;

[Collection(NetworkCollection.NAME)]
public class ResponderIntegrationTests
{
    private static readonly IEcdh s_dhFake = new FakeFixedKeyDh(ResponderValidKeysVector.EphemeralPrivateKey);

    [Fact]
    public void Given_ValidKeys_When_ResponderReadsActOne_Then_ItDoesntThrow()
    {
        // Arrange
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey, s_dhFake);

        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MaxMessageLength];

        try
        {
            // Act
            var (messageSize, handshakeHash, transport) = responder.ReadMessage(ResponderValidKeysVector.ActOneInput,
                messageBuffer);
            var message = messageBuffer[..messageSize];

            // compare bytes actOneOutput to initMessage
            Assert.Equal([], message.ToArray());
            Assert.Null(handshakeHash);
            Assert.Null(transport);
        }
        finally
        {
            responder.Dispose();
        }
    }

    [Fact]
    public void Given_ValidActOneRead_When_ResponderWritesActTwo_Then_OutputShouldBeValid()
    {
        // Arrange
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MaxMessageLength];

        try
        {
            // - Play ActOne
            _ = responder.ReadMessage(ResponderValidKeysVector.ActOneInput, messageBuffer);

            // Act
            var (messageSize, handshakeHash, transport) = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty),
                messageBuffer);
            var message = messageBuffer[..messageSize];

            // make sure reply is empty
            Assert.Equal(ResponderValidKeysVector.ActTwoOutput, message.ToArray());
            Assert.Null(handshakeHash);
            Assert.Null(transport);
        }
        finally
        {
            responder.Dispose();
        }
    }

    [Fact]
    public void Given_ValidActTwoMessage_When_ResponderReadsActThree_Then_ItDoesntThrow()
    {
        // Arrange
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MaxMessageLength];

        try
        {
            // - Play ActOne
            _ = responder.ReadMessage(ResponderValidKeysVector.ActOneInput, messageBuffer);
            // - Play ActTwo
            _ = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

            // Act
            var (messageSize, handshakeHash, transport) = responder.ReadMessage(ResponderValidKeysVector.ActThreeInput,
                messageBuffer);
            var message = messageBuffer[..messageSize];

            // compare bytes actThreeOutput to initMessage
            Assert.Equal([], message.ToArray());
            Assert.NotNull(handshakeHash);
            Assert.NotNull(transport);
        }
        finally
        {
            responder.Dispose();
        }
    }

    [Fact]
    public void Given_ValidActTwoRead_When_InitiatorWritesActThree_Then_KeysShouldBeValid()
    {
        // Arrange
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey, s_dhFake);
        Span<byte> messageBuffer = stackalloc byte[ProtocolConstants.MaxMessageLength];
        const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.NonPublic;

        try
        {
            // - Play ActOne
            _ = responder.ReadMessage(ResponderValidKeysVector.ActOneInput, messageBuffer);
            // - Play ActTwo
            _ = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
            // - Play ActThree
            var (_, _, transport) = responder.ReadMessage(ResponderValidKeysVector.ActThreeInput, messageBuffer);
            Assert.NotNull(transport);

            // Act
            // Get rk
            var c1 = ((CipherState?)transport
                         .GetType()
                         .GetField("_sendingKey", FLAGS)?
                         .GetValue(transport) ?? throw new MissingFieldException("_sendingKey"))
                     ?? throw new NullReferenceException("_sendingKey");
            var rk = ((SecureMemory?)c1
                         .GetType()
                         .GetField("_k", FLAGS)?
                         .GetValue(c1) ?? throw new MissingFieldException("_sendingKey._k"))
                     ?? throw new NullReferenceException("_sendingKey._k");
            // Get sk
            var c2 = ((CipherState?)transport
                         .GetType()
                         .GetField("_receivingKey", FLAGS)?
                         .GetValue(transport) ?? throw new MissingFieldException("_receivingKey"))
                     ?? throw new NullReferenceException("_receivingKey");
            var sk = ((SecureMemory?)c2
                         .GetType()
                         .GetField("_k", FLAGS)?
                         .GetValue(c2) ?? throw new MissingFieldException("_receivingKey._k"))
                     ?? throw new NullReferenceException("_receivingKey._k");

            Assert.Equal(((Span<byte>)ResponderValidKeysVector.OutputRk).ToArray(), ((Span<byte>)rk).ToArray());
            Assert.Equal(((Span<byte>)ResponderValidKeysVector.OutputSk).ToArray(), ((Span<byte>)sk).ToArray());
        }
        finally
        {
            responder.Dispose();
        }
    }

    [Theory]
    [InlineData("00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c",
        "Noise message must be equal to 50 bytes in length.")]
    [InlineData("01036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a",
        "Invalid handshake version.")]
    [InlineData("00046360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a",
        "Invalid public key")]
    [InlineData("00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6b",
        "Decryption failed.")]
    public void Given_InvalidActOneMessage_When_ResponderReadsActOne_Then_Throws(string actOneInput,
                                                                                 string exceptionMessage)
    {
        // Arrange
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey, s_dhFake);
        var messageBuffer = new byte[ProtocolConstants.MaxMessageLength];
        var inputBytes = Convert.FromHexString(actOneInput);

        try
        {
            // Act
            var exception = Assert.ThrowsAny<Exception>(() => responder.ReadMessage(inputBytes, messageBuffer));
            Assert.Equal(exceptionMessage, exception.Message);
        }
        finally
        {
            responder.Dispose();
        }
    }

    [Theory]
    [InlineData("00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139", "Noise message must be equal to 66 bytes in length.")]
    [InlineData("00c9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139ba", "Decryption failed.")]
    [InlineData("00bfe3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa2235536ad09a8ee351870c2bb7f78b754a26c6cef79a98d25139c856d7efd252c2ae73c", "Invalid public key")]
    [InlineData("00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139bb", "Decryption failed.")]
    public void Given_InvalidActThreeMessage_When_ResponderReadsActThree_Then_Throws(string actOneInput,
                                                                                     string exceptionMessage)
    {
        // Arrange
        var responder = new HandshakeState(false, ResponderValidKeysVector.LocalStaticPrivateKey,
                                           ResponderValidKeysVector.LocalStaticPublicKey, s_dhFake);
        var messageBuffer = new byte[ProtocolConstants.MaxMessageLength];
        var inputBytes = Convert.FromHexString(actOneInput);

        try
        {
            // - Play ActOne
            _ = responder.ReadMessage(ResponderValidKeysVector.ActOneInput, messageBuffer);
            // - Play ActTwo
            _ = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

            // Act
            var exception = Assert.ThrowsAny<Exception>(() => responder.ReadMessage(inputBytes, messageBuffer));
            Assert.Equal(exceptionMessage, exception.Message);
        }
        finally
        {
            responder.Dispose();
        }
    }
}