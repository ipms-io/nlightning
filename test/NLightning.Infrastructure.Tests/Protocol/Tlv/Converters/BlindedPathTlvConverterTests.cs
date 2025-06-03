using NBitcoin;

namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Infrastructure.Protocol.Tlv.Converters;

public class BlindedPathTlvConverterTests
{
    [Fact]
    public void Given_BlindedPathTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var pubkey = new Key().PubKey.ToBytes();
        var expectedBaseTlv = new BaseTlv(0, pubkey);
        var expectedBlindedPathTlv = new BlindedPathTlv(pubkey);
        var converter = new BlindedPathTlvConverter();

        // Act
        var baseTlv = converter.ConvertToBase(expectedBlindedPathTlv);
        var blindedPathTlv = converter.ConvertFromBase(expectedBaseTlv);

        // Assert
        Assert.Equal(expectedBlindedPathTlv, blindedPathTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}