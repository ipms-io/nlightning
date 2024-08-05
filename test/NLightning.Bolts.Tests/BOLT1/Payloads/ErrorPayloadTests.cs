using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Payloads;

using Bolts.BOLT1.Payloads;
using Common.Types;
using Common.BitUtils;

public class ErrorPayloadTests
{
    [Fact]
    public void Given_ChannelIdAndData_When_Constructing_Then_PayloadIsValid()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var data = Encoding.UTF8.GetBytes("Test message");

        // Act
        var errorPayload = new ErrorPayload(channelId, data);

        // Assert
        Assert.Equal(channelId, errorPayload.ChannelId);
        Assert.Equal(data, errorPayload.Data);
    }

    [Fact]
    public async Task Given_ValidPayload_When_Serializing_Then_ReturnsCorrectValues()
    {
        // Arrange
        var errorPayload = new ErrorPayload(ChannelId.Zero);
        using var memoryStream = new MemoryStream();

        // Act
        await errorPayload.SerializeAsync(memoryStream);

        // Assert
        memoryStream.Seek(0, SeekOrigin.Begin);
        var expectedLengthBytes = new byte[2];
        await memoryStream.ReadAsync(expectedLengthBytes.AsMemory(0, 2));

        Assert.Equal(0, EndianBitConverter.ToUInt16BE(expectedLengthBytes));
    }
}