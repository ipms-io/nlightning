using System.Reflection;

namespace NLightning.Bolts.Tests.BOLT1.Services;

using Bolts.BOLT1.Services;
using Bolts.BOLT8.Interfaces;
using Bolts.Factories;
using Bolts.Interfaces;

public class MessageServiceTests
{
    [Fact]
    public async Task Given_Message_When_SendMessageAsync_IsCalled_Then_TransportServiceWritesMessage()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(transportServiceMock.Object);
        var messageMock = new Mock<IMessage>();

        // Act
        await messageService.SendMessageAsync(messageMock.Object);

        // Assert
        transportServiceMock.Verify(t => t.WriteMessageAsync(messageMock.Object, It.IsAny<CancellationToken>()), Times.Once());
    }

    [Fact]
    public async Task Given_ReceivedMessage_When_ReceiveMessageAsync_IsInvoked_Then_MessageReceivedEventIsRaised()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(transportServiceMock.Object);
        var stream = new MemoryStream();
        var pingMessage = MessageFactory.CreatePingMessage();
        await pingMessage.SerializeAsync(stream);
        stream.Position = 0;

        var eventRaised = false;

        messageService.MessageReceived += (sender, args) => eventRaised = true;

        // Act
        var method = messageService.GetType().GetMethod("ReceiveMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(messageService, [messageService, stream]);

        // Assert
        Assert.True(eventRaised);
    }

    [Fact]
    public void Given_TransportServiceConnectionState_When_CheckingIsConnected_Then_ReturnsCorrectValue()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        transportServiceMock.Setup(t => t.IsConnected).Returns(true);
        var messageService = new MessageService(transportServiceMock.Object);

        // Act
        var isConnected = messageService.IsConnected;

        // Assert
        Assert.True(isConnected);
    }

    [Fact]
    public async Task Given_MessageService_When_Dispose_IsCalled_Then_TransportServiceIsDisposed()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(transportServiceMock.Object);

        // Act
        messageService.Dispose();

        // Assert
        transportServiceMock.Verify(t => t.Dispose(), Times.Once());

        await Assert.ThrowsAnyAsync<ObjectDisposedException>(() => messageService.SendMessageAsync(null));
    }
}