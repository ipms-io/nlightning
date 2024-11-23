using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT2.Messages;

using Bolts.BOLT2.Messages;
using Bolts.BOLT2.Payloads;
using Common.TLVs;
using Common.Types;
using Exceptions;
using Utils;

public class ChannelReestablishMessageTests
{
    #region Deserialize
    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsChannelReestablishMessage()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedNextCommitmentNumber = 1UL;
        var expectedNextRevocationNumber = 2UL;
        var expectedYourLastPerCommitmentSecret = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var expectedMyCurrentPerCommitmentPoint = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000002567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75".ToByteArray());

        // Act
        var message = await ChannelReestablishMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedNextCommitmentNumber, message.Payload.NextCommitmentNumber);
        Assert.Equal(expectedNextRevocationNumber, message.Payload.NextRevocationNumber);
        Assert.Equal(expectedYourLastPerCommitmentSecret, message.Payload.YourLastPerCommitmentSecret);
        Assert.Equal(expectedMyCurrentPerCommitmentPoint, message.Payload.MyCurrentPerCommitmentPoint);
        Assert.Null(message.Extension);
    }

    [Fact]
    public async Task Given_ValidStream_When_DeserializeAsync_Then_ReturnsChannelReestablishMessageWithExtensions()
    {
        // Arrange
        var expectedChannelId = ChannelId.Zero;
        var expectedNextCommitmentNumber = 1UL;
        var expectedNextRevocationNumber = 2UL;
        var expectedYourLastPerCommitmentSecret = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var expectedMyCurrentPerCommitmentPoint = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var nextFundingTlv = new NextFundingTlv("567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray());
        var stream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000002567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750020567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA5".ToByteArray());

        // Act
        var message = await ChannelReestablishMessage.DeserializeAsync(stream);

        // Assert
        Assert.Equal(expectedChannelId, message.Payload.ChannelId);
        Assert.Equal(expectedNextCommitmentNumber, message.Payload.NextCommitmentNumber);
        Assert.Equal(expectedNextRevocationNumber, message.Payload.NextRevocationNumber);
        Assert.Equal(expectedYourLastPerCommitmentSecret, message.Payload.YourLastPerCommitmentSecret);
        Assert.Equal(expectedMyCurrentPerCommitmentPoint, message.Payload.MyCurrentPerCommitmentPoint);
        Assert.NotNull(message.Extension);
        Assert.NotNull(message.NextFundingTlv);
        Assert.Equal(nextFundingTlv, message.NextFundingTlv);
    }

    [Fact]
    public async Task Given_InvalidStreamContent_When_DeserializeAsync_Then_ThrowsMessageSerializationException()
    {
        // Arrange
        var invalidStream = new MemoryStream("000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000002567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750002".ToByteArray());

        // Act & Assert
        await Assert.ThrowsAsync<MessageSerializationException>(() => ChannelReestablishMessage.DeserializeAsync(invalidStream));
    }
    #endregion

    #region Serialize
    [Fact]
    public async Task Given_ValidPayload_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var nextCommitmentNumber = 1UL;
        var nextRevocationNumber = 2UL;
        var yourLastPerCommitmentSecret = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var myCurrentPerCommitmentPoint = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var message = new ChannelReestablishMessage(new ChannelReestablishPayload(channelId, nextCommitmentNumber, nextRevocationNumber, yourLastPerCommitmentSecret, myCurrentPerCommitmentPoint));
        var stream = new MemoryStream();
        var expectedBytes = "0088000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000002567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A75".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }

    [Fact]
    public async Task Given_ValidExtensions_When_SerializeAsync_Then_WritesCorrectDataToStream()
    {
        // Arrange
        var channelId = ChannelId.Zero;
        var nextCommitmentNumber = 1UL;
        var nextRevocationNumber = 2UL;
        var yourLastPerCommitmentSecret = "567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray();
        var myCurrentPerCommitmentPoint = new PubKey("02c93ca7dca44d2e45e3cc5419d92750f7fb3a0f180852b73a621f4051c0193a75".ToByteArray());
        var nextFundingTlv = new NextFundingTlv("567cbdadb00b825448b2e414487d73a97f657f0634166d3ab3f3a2cc1042eda5".ToByteArray());
        var message = new ChannelReestablishMessage(new ChannelReestablishPayload(channelId, nextCommitmentNumber, nextRevocationNumber, yourLastPerCommitmentSecret, myCurrentPerCommitmentPoint), nextFundingTlv);
        var stream = new MemoryStream();
        var expectedBytes = "0088000000000000000000000000000000000000000000000000000000000000000000000000000000010000000000000002567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA502C93CA7DCA44D2E45E3CC5419D92750F7FB3A0F180852B73A621F4051C0193A750020567CBDADB00B825448B2E414487D73A97F657F0634166D3AB3F3A2CC1042EDA5".ToByteArray();

        // Act
        await message.SerializeAsync(stream);
        stream.Position = 0;
        var result = new byte[stream.Length];
        _ = await stream.ReadAsync(result);

        // Assert
        Assert.Equal(expectedBytes, result);
    }
    #endregion
}