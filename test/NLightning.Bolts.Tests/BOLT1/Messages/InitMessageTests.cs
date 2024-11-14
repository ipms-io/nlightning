namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.BOLT9;
using Common.Constants;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class InitMessageTests
{
    [Fact]
    public async Task Given_ValidStreamWithPayloadAndExtension_When_DeserializeAsync_Then_ReturnsInitMessageWithCorrectData()
    {
        // Arrange
        var expectedPayload = new InitPayload(new Features());
        var expectedExtension = new TlvStream();
        var expectedTlv = new NetworksTlv([ChainConstants.MAIN]);
        expectedExtension.Add(expectedTlv);
        var stream = await CreateStreamFromPayloadAndExtensionAsync(expectedPayload, expectedExtension);

        // Act
        var initMessage = await InitMessage.DeserializeAsync(stream);

        // Assert
        Tlv? tlv = null;
        Assert.NotNull(initMessage);
        Assert.Equal(expectedPayload.Features.ToString(), initMessage.Payload.Features.ToString());
        var hasTlv = initMessage.Extension?.TryGetTlv(TlvConstants.NETWORKS, out tlv);
        Assert.True(hasTlv);
        Assert.Equal(expectedTlv.Value, tlv!.Value);
    }

    [Fact]
    public async Task Given_ValidStreamWithOnlyPayload_When_DeserializeAsync_Then_ReturnsInitMessageWithNullExtension()
    {
        // Arrange
        var expectedPayload = new InitPayload(new Features());
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // Act
        var initMessage = await InitMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(initMessage);
        Assert.Equal(expectedPayload.Features.ToString(), initMessage.Payload.Features.ToString());
        Assert.Null(initMessage.Extension);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("Invalid content"u8.ToArray());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => InitMessage.DeserializeAsync(invalidStream));
    }

    private static async Task<Stream> CreateStreamFromPayloadAndExtensionAsync(InitPayload payload, TlvStream extension)
    {
        var stream = new MemoryStream();
        await payload.SerializeAsync(stream);
        await extension.SerializeAsync(stream);
        stream.Position = 0;
        return stream;
    }

    [Fact]
    public async Task Given_ValidPayloadAndExtension_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var expectedExtension = new TlvStream();
        expectedExtension.Add(new NetworksTlv([ChainConstants.MAIN]));
        var message = new InitMessage(new InitPayload(new Features()), expectedExtension);
        var stream = new MemoryStream();
        var expectedBytes = "0x001000020200000202000100".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidPayloadOnly_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new InitMessage(new InitPayload(new Features()));
        var stream = new MemoryStream();
        var expectedBytes = "0x00100002020000020200".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}