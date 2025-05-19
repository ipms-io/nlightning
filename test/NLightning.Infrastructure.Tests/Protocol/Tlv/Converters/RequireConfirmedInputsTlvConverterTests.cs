namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Infrastructure.Protocol.Tlv.Converters;

public class RequireConfirmedInputsTlvConverterTests
{
    [Fact]
    public void Given_RequireConfirmedInputsTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var expectedBaseTlv = new BaseTlv(2);
        var expectedRequireConfirmedInputsTlv = new RequireConfirmedInputsTlv();
        var converter = new RequireConfirmedInputsTlvConverter();
    
        // Act
        var baseTlv = converter.ConvertToBase(expectedRequireConfirmedInputsTlv);
        var requireConfirmedInputsTlv = converter.ConvertFromBase(expectedBaseTlv);
    
        // Assert
        Assert.Equal(expectedRequireConfirmedInputsTlv, requireConfirmedInputsTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}