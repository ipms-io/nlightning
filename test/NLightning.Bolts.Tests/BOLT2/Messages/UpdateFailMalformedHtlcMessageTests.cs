using NLightning.Common.Messages;
using NLightning.Common.Messages.Payloads;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Common.Types;
using Utils;

public class UpdateFailMalformedHtlcMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsUpdateFailMalformedHtlcMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedId = 0UL;
        var expectedSha256OfOnion = new byte[32];
        ushort expectedFailureCode = 1;
        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001".ToByteArray());

        // Act
        var message = await UpdateFailMalformedHtlcMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedId, message.Payload.Id);
        Assert.Equal(expectedSha256OfOnion, message.Payload.Sha256OfOnion);
        Assert.Equal(expectedFailureCode, message.Payload.FailureCode);
        Assert.Null(message.Extension);
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayloadWith_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var id = 0UL;
        var sha256OfOnion = new byte[32];
        ushort failureCode = 1;
        var message = new UpdateFailMalformedHtlcMessage(new UpdateFailMalformedHtlcPayload(channelId, id, sha256OfOnion, failureCode));
        var stream = new MemoryStream();
        var expectedBytes = "00870000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000001".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
    #endregion
}