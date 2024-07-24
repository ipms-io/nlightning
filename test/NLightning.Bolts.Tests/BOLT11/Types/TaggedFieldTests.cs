namespace NLightning.Bolts.Tests.BOLT11.Types;

public class TaggedFieldTests
{
    [Fact]
    public async void Given_PaymentSecret_When_Serialized_Then_ResultIsCorrect()
    {
        // Arrange
        // var type = TaggedFieldTypesConstants.PaymentSecret;
        // var data = Convert.FromHexString("1111111111111111111111111111111111111111111111111111111111111111".Replace("0x", string.Empty));

        // // Act
        // var taggedField = new TaggedField(type, data);
        // var stream = new MemoryStream();
        // // await taggedField.SerializeAsync(stream);
        // stream.Position = 0;
        // var result = stream.ToArray();
        // Console.Write(Convert.ToHexString(result));

        // // Assert
        // Assert.Equal(type, taggedField.Type);
    }
    // secret       sp5zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zyg3zygs
    // payment hash pp5qqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqqqsyqcyq5rqwzqfqypq
    // description  dpl2pkx2ctnv5sxxmmwwd5kgetjypeh2ursdae8g6twvus8g6rfwvs8qun0dfjkxaq
    // features     9qrsgq
    // signature    357wnc5r2ueh7ck6q93dj32dlqnls087fxdwk8qakdyafkq3yap9us6v52vjjsrvywa6rt52cm9r9zqt8r2t7mlcwspyetp5h2tztugp
    // checksum     9lfyql
}