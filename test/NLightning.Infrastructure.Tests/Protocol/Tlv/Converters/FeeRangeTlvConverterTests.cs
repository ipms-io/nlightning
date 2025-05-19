namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Money;
using Domain.Protocol.Tlv;
using Infrastructure.Converters;
using Infrastructure.Protocol.Tlv.Converters;

public class FeeRangeTlvConverterTests
{
    [Fact]
    public void Given_FeeRangeTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var minFeeAmount = LightningMoney.Satoshis(1);
        var maxFeeAmount = LightningMoney.Satoshis(2);
        var tlvValue = new byte[sizeof(ulong) * 2];
        EndianBitConverter.GetBytesBigEndian(minFeeAmount.Satoshi).CopyTo(tlvValue, 0);
        EndianBitConverter.GetBytesBigEndian(maxFeeAmount.Satoshi).CopyTo(tlvValue, sizeof(ulong));
        var expectedBaseTlv = new BaseTlv(1, tlvValue);
        var expectedFeeRangeTlv = new FeeRangeTlv(minFeeAmount, maxFeeAmount);
        var converter = new FeeRangeTlvConverter();

        // Act
        var baseTlv = converter.ConvertToBase(expectedFeeRangeTlv);
        var feeRangeTlv = converter.ConvertFromBase(expectedBaseTlv);

        // Assert
        Assert.Equal(expectedFeeRangeTlv, feeRangeTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}