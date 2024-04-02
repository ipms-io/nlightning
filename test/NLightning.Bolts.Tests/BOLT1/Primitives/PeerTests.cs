using System.Reflection;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT1.Primitives;

using Bolts.BOLT1.Interfaces;
using Bolts.BOLT1.Messages;
using Bolts.BOLT1.Primitives;
using Bolts.Factories;
using NLightning.Bolts.BOLT1.Payloads;
using NLightning.Bolts.BOLT9;
using NLightning.Common.Constants;

public class PeerTests
{
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IPingPongService> _mockPingPongService = new();
    private readonly PeerAddress _peerAddress = new(new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7"), "127.0.0.1", 8080);
    private readonly NodeOptions _nodeOptions = new();

    [Fact]
    public void Given_OutboundPeer_When_Constructing_Then_InitMessageIsSent()
    {
        // Given
        var isInbound = false;

        _mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<InitMessage>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask)
                           .Verifiable();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        // When
        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound);

        // Then
        _mockMessageService.Verify();
    }

    [Fact]
    public void Given_MessageServiceIsNotConnected_When_PeerIsConstructed_Then_ThrowsException()
    {
        // Arrange
        // Simulate the message service being disconnected
        _mockMessageService.Setup(m => m.IsConnected).Returns(false);

        // Act & Assert
        var exception = Assert.Throws<Exception>(() =>
            new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound: false)
        );

        Assert.Equal("Failed to connect to peer", exception.Message);
    }

    [Fact]
    public async Task Given_InboundPeer_When_InitMessageIsNotReceivedWithinTimeout_Then_Disconnects()
    {
        // Given
        var isInbound = true;
        var disconnectEventRaised = false;
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        _nodeOptions.NetworkTimeout = TimeSpan.FromSeconds(1);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound);
        peer.DisconnectEvent += (sender, args) => disconnectEventRaised = true;

        // When
        await Task.Delay(_nodeOptions.NetworkTimeout.Add(TimeSpan.FromSeconds(1)));

        // Then
        Assert.True(disconnectEventRaised);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingValidInitMessage_Then_IsInitialized()
    {
        // Given
        var initMessage = MessageFactory.CreateInitMessage(_nodeOptions);
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound: true);

        // When
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(peer, [peer, initMessage]);

        // Then
        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        var value = field?.GetValue(peer);
        Assert.NotNull(value);
        Assert.True((bool)value);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingInvalidInitMessage_Then_Disconnects()
    {
        // Given
        var disconnectEventRaised = false;
        var pingMessage = MessageFactory.CreatePingMessage();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound: true);
        peer.DisconnectEvent += (sender, args) => disconnectEventRaised = true;

        // When & Then
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = Assert.ThrowsAny<Exception>(() => method?.Invoke(peer, [peer, pingMessage]));

        Assert.True(disconnectEventRaised);
        Assert.Equal("Failed to receive init message", exception.InnerException?.Message);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingIncompatibleFeatures_Then_Disconnects()
    {
        // Given
        var disconnectEventRaised = false;
        var features = _nodeOptions.GetNodeFeatures();
        features.SetFeature(Feature.GossipQueries, true);
        var extension = _nodeOptions.GetInitExtension();
        var initMessage = new InitMessage(new InitPayload(features), extension);

        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound: true);
        peer.DisconnectEvent += (sender, args) => disconnectEventRaised = true;

        // When & Then
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = Assert.ThrowsAny<Exception>(() => method?.Invoke(peer, [peer, initMessage]));

        Assert.True(disconnectEventRaised);
        Assert.Equal("Peer is not compatible", exception.InnerException?.Message);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingIncompatibleChain_Then_Disconnects()
    {
        // Given
        var disconnectEventRaised = false;
        var otherNodeOptions = new NodeOptions
        {
            ChainHashes = [ChainConstants.Regtest]
        };
        var initMessage = MessageFactory.CreateInitMessage(otherNodeOptions);

        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound: true);
        peer.DisconnectEvent += (sender, args) => disconnectEventRaised = true;

        // When & Then
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);

        var exception = Assert.ThrowsAny<Exception>(() => method?.Invoke(peer, [peer, initMessage]));

        Assert.True(disconnectEventRaised);
        Assert.Equal("Peer chain is not compatible", exception.InnerException?.Message);
    }

    [Fact]
    public void Given_Peer_When_ReceivingPingMessage_Then_SendsPongMessage()
    {
        // Given
        var isInbound = false;
        var pingMessage = MessageFactory.CreatePingMessage();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        _mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<PongMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound);

        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(peer, true);

        // When
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(peer, [peer, pingMessage]);

        // Then
        _mockMessageService.Verify();
    }

    [Fact]
    public void Given_Peer_When_ReceivingPongMessage_Then_PingPongServiceHandlesPong()
    {
        // Given
        var isInbound = false;
        var pongMessage = MessageFactory.CreatePongMessage(1);
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_nodeOptions, _mockMessageService.Object, _mockPingPongService.Object, _peerAddress, isInbound);

        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        field?.SetValue(peer, true);

        // When
        var method = peer.GetType().GetMethod("HandleMessageAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        method?.Invoke(peer, [peer, pongMessage]);

        // Then
        _mockPingPongService.Verify(p => p.HandlePong(It.IsAny<PongMessage>()), Times.Once());
    }
}