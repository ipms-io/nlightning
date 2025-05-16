namespace NLightning.Infrastructure.Tests.Protocol.Services;

using Domain.Protocol.Messages.Interfaces;
using Domain.Serialization.Messages;
using Domain.Transport;
using Infrastructure.Protocol.Services;

public class MessageServiceTests
{
    private readonly Mock<IMessageSerializer> _messageSerializerMock;
    
    public MessageServiceTests()
    {
        _messageSerializerMock = new Mock<IMessageSerializer>();
    }

    [Fact]
    public async Task Given_Message_When_SendMessageAsync_IsCalled_Then_TransportServiceWritesMessage()
    {
        // Given
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(_messageSerializerMock.Object, transportServiceMock.Object);
        var messageMock = new Mock<IMessage>();

        // When
        await messageService.SendMessageAsync(messageMock.Object);

        // Then
        transportServiceMock.Verify(t => t.WriteMessageAsync(messageMock.Object, It.IsAny<CancellationToken>()),
                                   Times.Once());
    }

    [Fact]
    public void Given_ReceivedMessage_When_ReceiveMessageAsync_IsInvoked_Then_MessageReceivedEventIsRaised()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageMock = new Mock<IMessage>();
        _messageSerializerMock.Setup(m => m.DeserializeMessageAsync(It.IsAny<Stream>()))
            .ReturnsAsync(messageMock.Object);
        
        var messageService = new MessageService(_messageSerializerMock.Object, transportServiceMock.Object);
        var stream = new MemoryStream();

        // Act & Assert
        var receivedMessage = Assert.Raises<IMessage?>(
            h => messageService.MessageReceived += h,
            h => messageService.MessageReceived -= h,
            () => {
                // Simulate transport service receiving a message
                transportServiceMock.Raise(t => t.MessageReceived += null, messageService, stream);
            });

        Assert.NotNull(receivedMessage.Arguments);
        Assert.Same(messageMock.Object, receivedMessage.Arguments);
    }

    [Fact]
    public void Given_TransportServiceConnectionState_When_CheckingIsConnected_Then_ReturnsCorrectValue()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        transportServiceMock.Setup(t => t.IsConnected).Returns(true);
        var messageService = new MessageService(_messageSerializerMock.Object, transportServiceMock.Object);

        // Act
        var isConnected = messageService.IsConnected;

        // Assert
        Assert.True(isConnected);
    }

    [Fact]
    public void Given_MessageService_When_Dispose_IsCalled_Then_TransportServiceIsDisposed()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(_messageSerializerMock.Object, transportServiceMock.Object);

        // Act
        messageService.Dispose();

        // Assert
        transportServiceMock.Verify(t => t.Dispose(), Times.Once());
    }
    
    [Fact]
    public async Task Given_DisposedMessageService_When_SendMessageAsync_IsCalled_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(_messageSerializerMock.Object, transportServiceMock.Object);
        var messageMock = new Mock<IMessage>();
        
        // Act
        messageService.Dispose();
        
        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => messageService.SendMessageAsync(messageMock.Object));
    }
}