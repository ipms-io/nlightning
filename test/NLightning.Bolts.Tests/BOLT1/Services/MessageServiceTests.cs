using System.Reflection;
using Microsoft.Extensions.Options;

namespace NLightning.Bolts.Tests.BOLT1.Services;

using Bolts.BOLT1.Services;
using Common.Factories;
using Common.Interfaces;
using Common.Messages.Payloads;
using Common.Options;

public class MessageServiceTests
{
    private readonly MessageFactory _messageFactory = new(Options.Create(new NodeOptions()));

    [Fact]
    public async Task Given_Message_When_SendMessageAsync_IsCalled_Then_TransportServiceWritesMessage()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(_messageFactory, transportServiceMock.Object);
        var messageMock = new Mock<IMessage>();

        // Act
        await messageService.SendMessageAsync(messageMock.Object);

        // Assert
        transportServiceMock.Verify(t => t.WriteMessageAsync(messageMock.Object, It.IsAny<CancellationToken>()),
                                    Times.Once());
    }

    [Fact]
    public async Task Given_ReceivedMessage_When_ReceiveMessageAsync_IsInvoked_Then_MessageReceivedEventIsRaised()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        var messageService = new MessageService(_messageFactory, transportServiceMock.Object);
        var stream = new MemoryStream();
        var pingMessage = _messageFactory.CreatePingMessage();
        await pingMessage.SerializeAsync(stream);
        stream.Position = 0;
        var pingPayload = pingMessage.Payload as PingPayload ?? throw new Exception("Unable to converto payload");

        var tcs = new TaskCompletionSource<bool>();

        // Act & Assert
        var receivedMessage = await Assert.RaisesAnyAsync<IMessage?>(
            e => messageService.MessageReceived += (sender, message) => EventCallback(sender, message, e),
            e => messageService.MessageReceived -= (sender, message) => EventCallback(sender, message, e),
            async () =>
            {
                var method = messageService.GetType().GetMethod("ReceiveMessage",
                                                                BindingFlags.NonPublic | BindingFlags.Instance);
                Assert.NotNull(method);
                method.Invoke(messageService, [messageService, stream]);
                await tcs.Task;
            });

        Assert.NotNull(receivedMessage.Arguments);
        var receivedPayload = receivedMessage.Arguments.Payload as PingPayload;
        Assert.NotNull(receivedPayload);
        Assert.Equal(pingPayload.BytesLength, receivedPayload.BytesLength);

        return;
        void EventCallback(object? sender, IMessage? message, EventHandler<IMessage?> testHandler)
        {
            Assert.True(tcs.TrySetResult(true));
            testHandler(sender, message);
        }
    }

    [Fact]
    public void Given_TransportServiceConnectionState_When_CheckingIsConnected_Then_ReturnsCorrectValue()
    {
        // Arrange
        var transportServiceMock = new Mock<ITransportService>();
        transportServiceMock.Setup(t => t.IsConnected).Returns(true);
        var messageService = new MessageService(_messageFactory, transportServiceMock.Object);

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
        var messageService = new MessageService(_messageFactory, transportServiceMock.Object);

        // Act
        messageService.Dispose();

        // Assert
        transportServiceMock.Verify(t => t.Dispose(), Times.Once());

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        await Assert.ThrowsAnyAsync<ObjectDisposedException>(() => messageService.SendMessageAsync(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}