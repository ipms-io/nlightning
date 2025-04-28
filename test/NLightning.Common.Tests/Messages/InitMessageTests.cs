namespace NLightning.Common.Tests.Messages;

using Common.Messages;
using Common.Messages.Payloads;
using Common.Node;
using Common.TLVs;
using Common.Types;
using Constants;
using Exceptions;
using Utils;

public class InitMessageTests
{
    [Fact]
    public async Task Given_ValidStreamWithPayloadAndExtension_When_DeserializeAsync_Then_ReturnsInitMessageWithCorrectData()
    {
        // Arrange
        var expectedPayload = new InitPayload(new FeatureSet());
        var expectedExtension = new TlvStream();
        var expectedTlv = new NetworksTlv([ChainConstants.MAIN]);
        expectedExtension.Add(expectedTlv);
        var stream = new MemoryStream("000202000002020001206FE28C0AB6F1B372C1A6A246AE63F74F931E8365E15A089C68D6190000000000".GetBytes());

        // Act
        var initMessage = await InitMessage.DeserializeAsync(stream);

        // Assert
        Tlv? tlv = null;
        Assert.NotNull(initMessage);
        Assert.Equal(expectedPayload.FeatureSet.ToString(), initMessage.Payload.FeatureSet.ToString());
        var hasTlv = initMessage.Extension?.TryGetTlv(TlvConstants.NETWORKS, out tlv);
        Assert.True(hasTlv);
        Assert.Equal(expectedTlv.Value, tlv!.Value);
    }

    [Fact]
    public async Task Given_ValidStreamWithOnlyPayload_When_DeserializeAsync_Then_ReturnsInitMessageWithNullExtension()
    {
        // Arrange
        var expectedPayload = new InitPayload(new FeatureSet());
        var stream = new MemoryStream("0002020000020200".GetBytes());

        // Act
        var initMessage = await InitMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(initMessage);
        Assert.Equal(expectedPayload.FeatureSet.ToString(), initMessage.Payload.FeatureSet.ToString());
        Assert.Null(initMessage.Extension);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("00020200000202000102".GetBytes());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => InitMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_ValidPayloadAndExtension_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new InitMessage(new InitPayload(new FeatureSet()), new NetworksTlv([ChainConstants.MAIN]));
        var stream = new MemoryStream();
        var expectedBytes = "0010000202000002020001206FE28C0AB6F1B372C1A6A246AE63F74F931E8365E15A089C68D6190000000000".GetBytes();

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
        var message = new InitMessage(new InitPayload(new FeatureSet()));
        var stream = new MemoryStream();
        var expectedBytes = "00100002020000020200".GetBytes();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}