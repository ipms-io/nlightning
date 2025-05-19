using NBitcoin;

namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Infrastructure.Protocol.Tlv.Converters;

public class NextFundingTlvConverterTests
{
    [Fact]
    public void Given_NextFundingTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var nextFundingTxId = uint256.Zero.ToBytes();
        var expectedBaseTlv = new BaseTlv(0, nextFundingTxId);
        var expectedNextFundingTlv = new NextFundingTlv(nextFundingTxId);
        var converter = new NextFundingTlvConverter();
    
        // Act
        var baseTlv = converter.ConvertToBase(expectedNextFundingTlv);
        var nextFundingTlv = converter.ConvertFromBase(expectedBaseTlv);
    
        // Assert
        Assert.Equal(expectedNextFundingTlv, nextFundingTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}