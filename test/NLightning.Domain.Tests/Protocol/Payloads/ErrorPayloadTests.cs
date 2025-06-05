using System.Text;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Tests.Protocol.Payloads;

using Domain.Protocol.Payloads;

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
}