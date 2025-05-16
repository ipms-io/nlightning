using System.Text;
using NLightning.Common.Utils;

namespace NLightning.Common.Tests.Messages;

using Common.Types;
using Constants;
using Mock;

public class BaseMessageTests
{
    [Fact]
    public async Task Given_ValidPayload_WhenSerializng_Then_ReturnsCorrectValues()
    {
        // Arrange
        var mockPayload = new Mock<IMessagePayload>();
        var payloadBytes = Encoding.UTF8.GetBytes("payload");
        var messageType = MessageTypes.WARNING;
        var memoryStream = new MemoryStream();
        var message = new FakeMessage(messageType, mockPayload.Object);

        mockPayload.Setup(p => p.SerializeAsync(It.IsAny<Stream>()))
                   .Callback<Stream>(stream =>
                   {
                       stream.Write(payloadBytes, 0, payloadBytes.Length);
                   }).Returns(Task.CompletedTask);

        // Act
        await message.SerializeAsync(memoryStream);

        // Assert
        memoryStream.Position = 0;
        var buffer = new byte[memoryStream.Length];
        _ = await memoryStream.ReadAsync(buffer);

        Assert.Equal(messageType, EndianBitConverter.ToUInt16BigEndian(buffer[..2]));

        Assert.True(buffer.Skip(2).SequenceEqual(payloadBytes));
    }

    [Fact]
    public async Task Given_ValidExtension_WhenSerializng_Then_ReturnsCorrectValues()
    {
        // Arrange
        var mockPayload = new Mock<IMessagePayload>();
        var messageType = MessageTypes.WARNING;
        var memoryStream = new MemoryStream();
        var extensionBytes = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var extension = new TlvStream();
        extension.Add(new Tlv(0x01, extensionBytes[2..]));
        var message = new FakeMessage(messageType, mockPayload.Object, extension);

        mockPayload.Setup(p => p.SerializeAsync(It.IsAny<Stream>())).Returns(Task.CompletedTask);

        // Act
        await message.SerializeAsync(memoryStream);

        // Assert
        memoryStream.Position = 0;
        var buffer = new byte[memoryStream.Length];
        _ = await memoryStream.ReadAsync(buffer);

        Assert.True(buffer.Skip(2).SequenceEqual(extensionBytes));
    }
}