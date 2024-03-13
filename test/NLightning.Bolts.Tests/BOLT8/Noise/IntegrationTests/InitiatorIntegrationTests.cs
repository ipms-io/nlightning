using System.Reflection;
using System.Text;
using NLightning.Bolts.BOLT8.Noise.Ciphers;
using NLightning.Bolts.BOLT8.Noise.Interfaces;
using NLightning.Bolts.BOLT8.Noise.Primitives;
using NLightning.Bolts.BOLT8.Noise.States;
using NLightning.Bolts.Tests.BOLT8.Noise.Mock;

namespace NLightning.Bolts.Tests.BOLT8.Noise.IntegrationTests;

using static TestUtils;

public partial class IntegrationTests
{
    private static readonly InitiatorValidKeys _validKeys = new();

    [Fact]
    public void Given_ValidKeys_When_InitiatorWritesActOne_Then_OutputShouldBeValid()
    {
        // Arrange
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _validKeys.LocalStaticPrivateKey, _validKeys.RemoteStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? transport;
        byte[]? handshakeHash;
        Span<byte> message;
        int messageSize;

        // Act
        (messageSize, handshakeHash, transport) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        message = messageBuffer.AsSpan(0, messageSize);

        // compare bytes actOneOutput to initMessage
        Assert.Equal(_validKeys.ActOneOutput, message.ToArray());
        Assert.Null(handshakeHash);
        Assert.Null(transport);
    }

    [Fact]
    public void Given_ValidActTwoMessage_When_InitiatorReadsActTwo_Then_ItDoesntThrow()
    {
        // Arrange
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _validKeys.LocalStaticPrivateKey, _validKeys.RemoteStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? transport;
        byte[]? handshakeHash;
        Span<byte> message;
        int messageSize;

        // - Play ActOne
        _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

        // Act
        (messageSize, handshakeHash, transport) = initiator.ReadMessage(_validKeys.ActTwoInput, messageBuffer);
        message = messageBuffer.AsSpan(0, messageSize);

        // make sure reply is empty
        Assert.Equal([], message.ToArray());
        Assert.Null(handshakeHash);
        Assert.Null(transport);
    }

    [Fact]
    public void Given_ValidActTwoRead_When_InitiatorWritesActThree_Then_OutputShouldBeValid()
    {
        // Arrange
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _validKeys.LocalStaticPrivateKey, _validKeys.RemoteStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? transport;
        byte[]? handshakeHash;
        Span<byte> message;
        int messageSize;

        // - Play ActOne
        _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        // - Play ActTwo
        _ = initiator.ReadMessage(_validKeys.ActTwoInput, messageBuffer);

        // Act
        (messageSize, handshakeHash, transport) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        message = messageBuffer.AsSpan(0, messageSize);

        // compare bytes actThreeOutput to initMessage
        Assert.Equal(_validKeys.ActThreeOutput, message.ToArray());
        Assert.NotNull(handshakeHash);
        Assert.NotNull(transport);
    }

    [Fact]
    public void Given_ValidActTwoRead_When_InitiatorWritesActThree_Then_KeysShouldBeValid()
    {
        // Arrange
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _validKeys.LocalStaticPrivateKey, _validKeys.RemoteStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        ITransport? transport;

        // - Play ActOne
        _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        // - Play ActTwo
        _ = initiator.ReadMessage(_validKeys.ActTwoInput, messageBuffer);
        // - Play ActThree
        (_, _, transport) = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);
        Assert.NotNull(transport);

        // Act
        // Get sk
        var c1 = ((CipherState<ChaCha20Poly1305>?)transport.GetType().GetField("_c1", flags)?.GetValue(transport) ?? throw new MissingFieldException("_c1")) ?? throw new ArgumentNullException("_c1");
        var sk = ((byte[]?)c1.GetType().GetField("_k", flags)?.GetValue(c1) ?? throw new MissingFieldException("_c1._k")) ?? throw new ArgumentNullException("_c1._k");
        // Get rk
        var c2 = ((CipherState<ChaCha20Poly1305>?)transport.GetType().GetField("_c2", flags)?.GetValue(transport) ?? throw new MissingFieldException("_c2")) ?? throw new ArgumentNullException("_c2");
        var rk = ((byte[]?)c2.GetType().GetField("_k", flags)?.GetValue(c2) ?? throw new MissingFieldException("_c2._k")) ?? throw new ArgumentNullException("_c2._k");

        Assert.Equal(_validKeys.OutputSk, sk);
        Assert.Equal(_validKeys.OutputRk, rk);
    }

    [Theory]
    [InlineData("0x0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730", "Noise message must be equal to 50 bytes in length.")]
    [InlineData("0x0102466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae", "Invalid handshake version.")]
    [InlineData("0x0004466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730ae", "Invalid public key")]
    [InlineData("0x0002466d7fcae563e5cb09a0d1870bb580344804617879a14949cf22285f1bae3f276e2470b93aac583c9ef6eafca3f730af", "Decryption failed.")]
    public void Given_InvalidActTwoMessage_When_InitiatorReadsActTwo_Then_Throws(string actTwoInput, string exceptionMessage)
    {
        // Arrange
        var protocol = new Protocol();
        var initiator = protocol.Create(true, _validKeys.LocalStaticPrivateKey, _validKeys.RemoteStaticPublicKey);

        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var setDh = initiator.GetType().GetMethod("SetDh", flags) ?? throw new MissingMethodException("SetDh");
        setDh.Invoke(initiator, [new FixedKeyDh(_validKeys.EphemeralPrivateKey)]);

        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        var inputBytes = GetBytes(actTwoInput);

        // - Play ActOne
        _ = initiator.WriteMessage(Encoding.ASCII.GetBytes(string.Empty), messageBuffer);

        // Act
        var exception = Assert.ThrowsAny<Exception>(() => initiator.ReadMessage(inputBytes, messageBuffer));
        Assert.Equal(exceptionMessage, exception.Message);
    }
}