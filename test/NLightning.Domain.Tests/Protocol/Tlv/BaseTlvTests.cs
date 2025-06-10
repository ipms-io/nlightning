using NLightning.Domain.Protocol.ValueObjects;

namespace NLightning.Domain.Tests.Protocol.Tlv;

using Domain.Protocol.Tlv;

public class BaseTlvTests
{
    [Fact]
    public void Given_TwoTlvsWithDifferentTypes_When_Compared_Then_AreNotEqual()
    {
        // Given
        var tlv1 = new BaseTlv(new BigSize(1), [0x01]);
        var tlv2 = new BaseTlv(new BigSize(2), [0x01]);

        // When
        var areEqual = tlv1.Equals(tlv2);

        // Then
        Assert.False(areEqual);
    }
}