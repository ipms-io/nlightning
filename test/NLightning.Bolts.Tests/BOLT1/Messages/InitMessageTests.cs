using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.BOLT9;
using Bolts.Exceptions;
using Common.Constants;
using Common.TLVs;
using Common.Types;
using Tests.Utils;

public class InitMessageTests
{
    [Fact]
    public async Task Given_ValidStreamWithPayloadAndExtension_When_DeserializeAsync_Then_ReturnsInitMessageWithCorrectData()
    {
        // Arrange
        var expectedFeatures = new Features().ToString();
        var expectedExtension = new TLVStream();
        var expectedTlv = new NetworksTLV([ChainConstants.Main]);
        expectedExtension.Add(expectedTlv);
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x001000020200000202000100"));

        // Act
        var message = await InitMessage.DeserializeAsync(stream);

        // Assert
        TLV? tlv = null;
        Assert.NotNull(message);
        Assert.Equal(expectedFeatures, message.Payload.Features.ToString());
        var hasTlv = message.Extension?.TryGetTlv(TLVConstants.NETWORKS, out tlv);
        Assert.True(hasTlv);
        Assert.Equal(expectedTlv.Value, tlv!.Value);
    }

    [Fact]
    public async Task Given_ValidStreamWithOnlyPayload_When_DeserializeAsync_Then_ReturnsInitMessageWithNullExtension()
    {
        // Arrange
        var expectedFeatures = new Features().ToString();
        var stream = new MemoryStream(TestHexConverter.ToByteArray("0x00100002020000020200"));

        // Act
        var message = await InitMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedFeatures, message.Payload.Features.ToString());
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => InitMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_ValidPayloadAndExtension_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var expectedExtension = new TLVStream();
        expectedExtension.Add(new NetworksTLV([ChainConstants.Main]));
        var message = new InitMessage(new InitPayload(new Features()), expectedExtension);
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x001000020200000202000100");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidPayloadOnly_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var message = new InitMessage(new InitPayload(new Features()));
        var stream = new MemoryStream();
        var expectedBytes = TestHexConverter.ToByteArray("0x00100002020000020200");

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}