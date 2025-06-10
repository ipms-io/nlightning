using NLightning.Domain.Protocol.ValueObjects;

namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Infrastructure.Protocol.Tlv.Converters;

public class NetworksTlvConverterTests
{
    [Fact]
    public void Given_NetworksTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var chainHash = BitcoinNetwork.Mainnet.ChainHash;
        var expectedBaseTlv = new BaseTlv(1, chainHash);
        var expectedNetworksTlv = new NetworksTlv([chainHash]);
        var converter = new NetworksTlvConverter();

        // Act
        var baseTlv = converter.ConvertToBase(expectedNetworksTlv);
        var networksTlv = converter.ConvertFromBase(expectedBaseTlv);

        // Assert
        Assert.Equal(expectedNetworksTlv, networksTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}