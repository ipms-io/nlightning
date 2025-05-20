using NBitcoin;

namespace NLightning.Infrastructure.Tests.Protocol.Tlv.Converters;

using Domain.Protocol.Tlv;
using Infrastructure.Protocol.Tlv.Converters;

public class UpfrontShutdownScriptTlvConverterTests
{
    [Fact]
    public void Given_UpfrontShutdownScriptTlvConverter_When_ConvertingToBaseTlvAndBack_ResultIsCorrect()
    {
        // Arrange
        var script = new Script([0x01, 0x02, 0x03]);
        var expectedBaseTlv = new BaseTlv(0, script.ToBytes());
        var expectedUpfrontShutdownScriptTlv = new UpfrontShutdownScriptTlv(script);
        var converter = new UpfrontShutdownScriptTlvConverter();

        // Act
        var baseTlv = converter.ConvertToBase(expectedUpfrontShutdownScriptTlv);
        var upfrontShutdownScriptTlv = converter.ConvertFromBase(expectedBaseTlv);

        // Assert
        Assert.Equal(expectedUpfrontShutdownScriptTlv, upfrontShutdownScriptTlv);
        Assert.Equal(expectedBaseTlv, baseTlv);
    }
}