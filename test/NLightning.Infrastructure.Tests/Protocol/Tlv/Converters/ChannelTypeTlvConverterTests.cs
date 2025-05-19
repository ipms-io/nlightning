namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Infrastructure.Protocol.Tlv.Converters;

public class ChannelTypeTlvConverterTests
{
    [Fact]
    public void Given_ChannelTypeTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        byte[] channelType = [0x01, 0x02, 0x03];
        var expectedBaseTlv = new BaseTlv(1, channelType);
        var expectedChannelTypeTlv = new ChannelTypeTlv(channelType);
        var converter = new ChannelTypeTlvConverter();
    
        // Act
        var baseTlv = converter.ConvertToBase(expectedChannelTypeTlv);
        var channelTypeTlv = converter.ConvertFromBase(expectedBaseTlv);
    
        // Assert
        Assert.Equal(expectedChannelTypeTlv, channelTypeTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}