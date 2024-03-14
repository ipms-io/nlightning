using System.Reflection;
using System.Text;
using NLightning.Bolts.BOLT8.Noise.Primitives;
using NLightning.Bolts.BOLT8.Noise.States;
using NLightning.Bolts.Tests.BOLT8.Noise.Mock;
using NLightning.Bolts.Tests.BOLT8.Noise.Utils;

namespace NLightning.Bolts.Tests.BOLT8.Noise.IntegrationTests;

using static TestUtils;

public partial class ResponderTests
{
    private static readonly ResponderValidKeysUtil _validKeys = new();

    [Fact]
    public void Given_ValidKeys_When_ResponderReadsActOne_Then_ItDoesntThrow()
    {
        // Arrange
        var protocol = new Protocol();
        var responder = protocol.Create(false, _validKeys.LocalStaticPrivateKey, _validKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        Transport? transport;
        byte[]? handshakeHash;
        Span<byte> message;
        int messageSize;

        // Act
        (messageSize, handshakeHash, transport) = responder.ReadMessage(_validKeys.ActOneInput, messageBuffer);
        message = messageBuffer.AsSpan(0, messageSize);

        // compare bytes actOneOutput to initMessage
        Assert.Equal([], message.ToArray());
        Assert.Null(handshakeHash);
        Assert.Null(transport);
    }

    [Fact]
    public void Given_ValidActOneRead_When_ResponderWritesActTwo_Then_OutputShouldBeValid()
    {
        // Arrange
        var protocol = new Protocol();
        var responder = protocol.Create(false, _validKeys.LocalStaticPrivateKey, _validKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        Transport? transport;
        byte[]? handshakeHash;
        Span<byte> message;
        int messageSize;

        // - Play ActOne
        _ = responder.ReadMessage(_validKeys.ActOneInput, messageBuffer);

        // Act
        (messageSize, handshakeHash, transport) = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        message = messageBuffer.AsSpan(0, messageSize);

        // make sure reply is empty
        Assert.Equal(_validKeys.ActTwoOutput, message.ToArray());
        Assert.Null(handshakeHash);
        Assert.Null(transport);
    }

    [Fact]
    public void Given_ValidActTwoMessage_When_ResponderReadsActThree_Then_ItDoesntThrow()
    {
        // Arrange
        var protocol = new Protocol();
        var responder = protocol.Create(false, _validKeys.LocalStaticPrivateKey, _validKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        Transport? transport;
        byte[]? handshakeHash;
        Span<byte> message;
        int messageSize;

        // - Play ActOne
        _ = responder.ReadMessage(_validKeys.ActOneInput, messageBuffer);
        // - Play ActTwo
        _ = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

        // Act
        (messageSize, handshakeHash, transport) = responder.ReadMessage(_validKeys.ActThreeInput, messageBuffer);
        message = messageBuffer.AsSpan(0, messageSize);

        // compare bytes actThreeOutput to initMessage
        Assert.Equal([], message.ToArray());
        Assert.NotNull(handshakeHash);
        Assert.NotNull(transport);
    }

    [Fact]
    public void Given_ValidActTwoRead_When_InitiatorWritesActThree_Then_KeysShouldBeValid()
    {
        // Arrange
        var protocol = new Protocol();
        var responder = protocol.Create(false, _validKeys.LocalStaticPrivateKey, _validKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        Transport? transport;

        // - Play ActOne
        _ = responder.ReadMessage(_validKeys.ActOneInput, messageBuffer);
        // - Play ActTwo
        _ = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        // - Play ActThree
        (_, _, transport) = responder.ReadMessage(_validKeys.ActThreeInput, messageBuffer);
        Assert.NotNull(transport);

        // Act
        // Get rk
        var c1 = ((CipherState?)transport.GetType().GetField("_sendingKey", flags)?.GetValue(transport) ?? throw new MissingFieldException("_sendingKey")) ?? throw new ArgumentNullException("_sendingKey");
        var rk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_sendingKey._k")) ?? throw new ArgumentNullException("_sendingKey._k");
        // Get sk
        var c2 = ((CipherState?)transport.GetType().GetField("_receivingKey", flags)?.GetValue(transport) ?? throw new MissingFieldException("_receivingKey")) ?? throw new ArgumentNullException("_receivingKey");
        var sk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_receivingKey._k")) ?? throw new ArgumentNullException("_receivingKey._k");

        Assert.Equal(_validKeys.OutputRk, rk);
        Assert.Equal(_validKeys.OutputSk, sk);
    }

    [Theory]
    [InlineData("0x00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c", "Noise message must be equal to 50 bytes in length.")]
    [InlineData("0x01036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a", "Invalid handshake version.")]
    [InlineData("0x00046360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6a", "Invalid public key")]
    [InlineData("0x00036360e856310ce5d294e8be33fc807077dc56ac80d95d9cd4ddbd21325eff73f70df6086551151f58b8afe6c195782c6b", "Decryption failed.")]
    public void Given_InvalidActOneMessage_When_ResponderReadsActOne_Then_Throws(string actOneInput, string exceptionMessage)
    {
        // Arrange
        var protocol = new Protocol();
        var responder = protocol.Create(false, _validKeys.LocalStaticPrivateKey, _validKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        var inputBytes = GetBytes(actOneInput);

        // Act
        var exception = Assert.ThrowsAny<Exception>(() => responder.ReadMessage(inputBytes, messageBuffer));
        Assert.Equal(exceptionMessage, exception.Message);
    }

    [Theory]
    [InlineData("0x00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139", "Noise message must be equal to 66 bytes in length.")]
    [InlineData("0x00c9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139ba", "Decryption failed.")]
    [InlineData("0x00bfe3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa2235536ad09a8ee351870c2bb7f78b754a26c6cef79a98d25139c856d7efd252c2ae73c", "Invalid public key")]
    [InlineData("0x00b9e3a702e93e3a9948c2ed6e5fd7590a6e1c3a0344cfc9d5b57357049aa22355361aa02e55a8fc28fef5bd6d71ad0c38228dc68b1c466263b47fdf31e560e139bb", "Decryption failed.")]
    public void Given_InvalidActThreeMessage_When_ResponderReadsActThree_Then_Throws(string actOneInput, string exceptionMessage)
    {
        // Arrange
        var protocol = new Protocol();
        var responder = protocol.Create(false, _validKeys.LocalStaticPrivateKey, _validKeys.LocalStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = responder.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(responder, [new FakeFixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        var inputBytes = GetBytes(actOneInput);

        // - Play ActOne
        _ = responder.ReadMessage(_validKeys.ActOneInput, messageBuffer);
        // - Play ActTwo
        _ = responder.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

        // Act
        var exception = Assert.ThrowsAny<Exception>(() => responder.ReadMessage(inputBytes, messageBuffer));
        Assert.Equal(exceptionMessage, exception.Message);
    }
}