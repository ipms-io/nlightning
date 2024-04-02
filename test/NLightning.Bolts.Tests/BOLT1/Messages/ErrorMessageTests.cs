using System.Text;

namespace NLightning.Bolts.Tests.BOLT1.Messages;

using BOLT1.Mock;
using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.Exceptions;

public class ErrorMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_IsCalled_Then_ReturnsErrorMessageWithCorrectPayload()
    {
        // Given
        var expectedPayload = new ErrorPayload("Error message!");
        var stream = await Helpers.CreateStreamFromPayloadAsync(expectedPayload);

        // When
        var errorMessage = await ErrorMessage.DeserializeAsync(stream);

        // Then
        Assert.NotNull(errorMessage);
        Assert.Equal(expectedPayload.ChannelId, errorMessage.Payload.ChannelId);
        Assert.Equal(expectedPayload.Data, errorMessage.Payload.Data);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_IsCalled_Then_ThrowsMessageSerializationException()
    {
        // Given
        var invalidStream = new MemoryStream(Encoding.UTF8.GetBytes("Invalid content"));

        // When & Then
        await Assert.ThrowsAsync<MessageSerializationException>(() => ErrorMessage.DeserializeAsync(invalidStream));
    }

    [Fact]
    public async Task Given_StreamReadError_When_DeserializeAsync_IsCalled_Then_ThrowsIOException()
    {
        // Given
        var brokenStream = new FakeBrokenStream(); // You would need to mock or implement a stream that simulates a read error.

        // When & Then
        await Assert.ThrowsAsync<MessageSerializationException>(() => ErrorMessage.DeserializeAsync(brokenStream));
    }
}