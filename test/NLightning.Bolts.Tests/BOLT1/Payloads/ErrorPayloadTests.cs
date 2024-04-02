using System.Text;
using NLightning.Bolts.BOLT1.Payloads;
using NLightning.Common.Types;
using NLightning.Common.Utils;

namespace NLightning.Bolts.Tests.BOLT1.Payloads;

public class ErrorPayloadTests
{
    [Fact]
    public void Constructor_WithChannelIdAndData_InitializesPropertiesCorrectly()
    {
        // Given
        var channelId = ChannelId.Zero;
        var data = Encoding.UTF8.GetBytes("Test message");

        // When
        var errorPayload = new ErrorPayload(channelId, data);

        // Then
        Assert.Equal(channelId, errorPayload.ChannelId);
        Assert.Equal(data, errorPayload.Data);
    }

    [Fact]
    public async Task SerializeAsync_WithDataNull_WritesZeroLengthToStream()
    {
        // Given
        var errorPayload = new ErrorPayload(ChannelId.Zero);
        using var memoryStream = new MemoryStream();

        // When
        await errorPayload.SerializeAsync(memoryStream);

        // Then
        memoryStream.Seek(0, SeekOrigin.Begin);
        var expectedLengthBytes = new byte[2];
        await memoryStream.ReadAsync(expectedLengthBytes, 0, 2);

        Assert.Equal(0, EndianBitConverter.ToUInt16BE(expectedLengthBytes));
    }
}