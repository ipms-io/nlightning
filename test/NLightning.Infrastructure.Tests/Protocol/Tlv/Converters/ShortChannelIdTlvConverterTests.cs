using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Domain.ValueObjects;
using Infrastructure.Protocol.Tlv.Converters;

public class ShortChannelIdTlvConverterTests
{
    [Fact]
    public void Given_ShortChannelIdTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var shortChannelId = new ShortChannelId(1234, 5, 6);
        var expectedBaseTlv = new BaseTlv(1, shortChannelId);
        var expectedShortChannelIdTlv = new ShortChannelIdTlv(shortChannelId);
        var converter = new ShortChannelIdTlvConverter();

        // Act
        var baseTlv = converter.ConvertToBase(expectedShortChannelIdTlv);
        var shortChannelIdTlv = converter.ConvertFromBase(expectedBaseTlv);

        // Assert
        Assert.Equal(expectedShortChannelIdTlv, shortChannelIdTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}