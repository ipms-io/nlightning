namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.Types;
using Utils;

public class TxSignaturesMessageTests
{
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsTxSignaturesMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        byte[] expectedTxId = ChannelId.Zero; // 32 zeros
        var expectedWitnesses = new List<Witness>
        {
            new([0xFF, 0xFF, 0xFF, 0xFD])
        };
        var stream = new MemoryStream("0000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000010004FFFFFFFD".ToByteArray());

        // Act
        var message = await TxSignaturesMessage.DeserializeAsync(stream);

        // Assert
        Assert.NotNull(message);
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedTxId, message.Payload.TxId);
        Assert.Equal(expectedWitnesses.Count, message.Payload.Witnesses.Count);
        Assert.Equal(expectedWitnesses[0].WitnessData, message.Payload.Witnesses[0].WitnessData);
    }

    [Fact]
    public async Task Given_GivenValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        byte[] txId = ChannelId.Zero; // 32 zeros
        var witnesses = new List<Witness>
        {
            new([0xFF, 0xFF, 0xFF, 0xFD])
        };
        var message = new TxSignaturesMessage(new TxSignaturesPayload(channelId, txId, witnesses));
        var stream = new MemoryStream();
        var expectedBytes = "00470000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000010004FFFFFFFD".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
}