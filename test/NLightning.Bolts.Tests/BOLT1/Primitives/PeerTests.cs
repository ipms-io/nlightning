using System.Reflection;

namespace NLightning.Bolts.Tests.BOLT1.Primitives;

using Bolts.BOLT1.Interfaces;
using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Payloads;
using Bolts.BOLT1.Primitives;
using Bolts.BOLT9;
using Common.Constants;
using Exceptions;
using Factories;

public class PeerTests
{
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IPingPongService> _mockPingPongService = new();
    private readonly NodeOptions _nodeOptions = new();

    [Fact]
    public void Given_OutboundPeer_When_Constructing_Then_InitMessageIsSent()
    {
        // Arrange
        var isInbound = false;

        _mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<InitMessage>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask)
                           .Verifiable();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        // Act
        _ = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound);

        // Assert
        _mockMessageService.Verify();
    }

    [Fact]
    public void Given_MessageServiceIsNotConnected_When_PeerIsConstructed_Then_ThrowsException()
    {
        // Arrange
        // Simulate the message service being disconnected
        _mockMessageService.Setup(m => m.IsConnected).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<ConnectionException>(() =>
            new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound: false)
        );

        Assert.Equal("Failed to connect to peer", exception.Message);
    }

    [Fact]
    public async Task Given_InboundPeer_When_InitMessageIsNotReceivedWithinTimeout_Then_Disconnects()
    {
        // Arrange
        var disconnectEventRaised = false;
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        _nodeOptions.NetworkTimeout = TimeSpan.FromSeconds(1);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act
        await Task.Delay(_nodeOptions.NetworkTimeout.Add(TimeSpan.FromSeconds(1)));

        // Assert
        Assert.True(disconnectEventRaised);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingValidInitMessage_Then_IsInitialized()
    {
        // Arrange
        var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound: true);

        // Act
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(peer, [peer, initMessage]);

        // Assert
        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = field?.GetValue(peer);
        Assert.NotNull(value);
        Assert.True((bool)value);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingInvalidInitMessage_Then_Disconnects()
    {
        // Arrange
        var disconnectEventRaised = false;
        var pingMessage = MessageFactory.CreatePingMessage();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound: true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act & Assert
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = Assert.ThrowsAny<Exception>(() => method?.Invoke(peer, [peer, pingMessage]));

        Assert.True(disconnectEventRaised);
        Assert.Equal("Failed to receive init message", exception.InnerException?.Message);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingIncompatibleFeatures_Then_Disconnects()
    {
        // Arrange
        var disconnectEventRaised = false;
        var features = _nodeOptions.GetNodeFeatures();
        features.SetFeature(Feature.GOSSIP_QUERIES, true);
        var initMessage = new InitMessage(new InitPayload(features), _nodeOptions.GetInitTlvs());

        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound: true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act & Assert
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = Assert.ThrowsAny<Exception>(() => method?.Invoke(peer, [peer, initMessage]));

        Assert.True(disconnectEventRaised);
        Assert.Equal("Peer is not compatible", exception.InnerException?.Message);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingIncompatibleChain_Then_Disconnects()
    {
        // Arrange
        var disconnectEventRaised = false;
        var otherNodeOptions = new NodeOptions
        {
            ChainHashes = [ChainConstants.REGTEST]
        };
        var initMessage = MessageFactory.CreateInitMessage(otherNodeOptions);

        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound: true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act & Assert
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = Assert.ThrowsAny<Exception>(() => method?.Invoke(peer, [peer, initMessage]));

        Assert.True(disconnectEventRaised);
        Assert.Equal("Peer chain is not compatible", exception.InnerException?.Message);
    }

    [Fact]
    public void Given_Peer_When_ReceivingPingMessage_Then_SendsPongMessage()
    {
        // Arrange
        var isInbound = false;
        var pingMessage = MessageFactory.CreatePingMessage();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        _mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<PongMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, isInbound);

        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(peer, true);

        // Act
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(peer, [peer, pingMessage]);

        // Assert
        _mockMessageService.Verify();
    }

    [Fact]
    public void Given_Peer_When_ReceivingPongMessage_Then_PingPongServiceHandlesPong()
    {
        // Arrange
        var pongMessage = MessageFactory.CreatePongMessage(1);
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, false);

        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(peer, true);

        // Act
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(peer, [peer, pongMessage]);

        // Assert
        _mockPingPongService.Verify(p => p.HandlePong(It.IsAny<PongMessage>()), Times.Once());
    }
}