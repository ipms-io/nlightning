namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Money;
using Domain.Protocol.Tlv;
using Infrastructure.Converters;
using Infrastructure.Protocol.Tlv.Converters;

public class FundingOutputContributionTlvConverterTests
{
    [Fact]
    public void Given_FundingOutputContributionTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var amount = LightningMoney.Satoshis(100_000);
        var expectedBaseTlv = new BaseTlv(0, EndianBitConverter.GetBytesBigEndian(amount.Satoshi));
        var expectedFundingOutputContributionTlv = new FundingOutputContributionTlv(amount);
        var converter = new FundingOutputContributionTlvConverter();
    
        // Act
        var baseTlv = converter.ConvertToBase(expectedFundingOutputContributionTlv);
        var fundingOutputContributionTlv = converter.ConvertFromBase(expectedBaseTlv);
    
        // Assert
        Assert.Equal(expectedFundingOutputContributionTlv, fundingOutputContributionTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}