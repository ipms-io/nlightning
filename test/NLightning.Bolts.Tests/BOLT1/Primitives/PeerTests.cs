using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT1.Primitives;

using Common.Constants;
using Common.Enums;
using Common.Exceptions;
using Common.Factories;
using Common.Interfaces;
using Common.Managers;
using Common.Messages;
using Common.Messages.Payloads;
using Common.Node;
using Common.Options;
using Common.Types;
using TestCollections;
using Utils;

[Collection(ConfigManagerCollection.NAME)]
public class PeerTests
{
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IPingPongService> _mockPingPongService = new();
    private readonly MessageFactory _messageFactory = new();
    private readonly Mock<ILogger<Peer>> _mockLogger = new();
    private readonly IOptions<NodeOptions> _nodeOptions = new OptionsWrapper<NodeOptions>(new NodeOptions());
    private readonly PeerAddress _peerAddress = new(new Key().PubKey, "127.0.0.1", 1234);

    [Fact]
    public void Given_OutboundPeer_When_Constructing_Then_InitMessageIsSent()
    {
        // Arrange
        const bool IS_INBOUND = false;

        _mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<InitMessage>(), It.IsAny<CancellationToken>()))
                           .Returns(Task.CompletedTask)
                           .Verifiable();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        // Act
        _ = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory, _mockLogger.Object,
                     _nodeOptions, _peerAddress, IS_INBOUND);

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
            new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory, _mockLogger.Object,
                     _nodeOptions, _peerAddress, false)
        );

        Assert.Equal("Failed to connect to peer", exception.Message);
    }

    [Fact]
    public async Task Given_InboundPeer_When_InitMessageIsNotReceivedWithinTimeout_Then_Disconnects()
    {
        // Arrange
        var disconnectEventRaised = false;
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        ConfigManager.Instance.NetworkTimeout = TimeSpan.FromSeconds(1);

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act
        await Task.Delay(ConfigManager.Instance.NetworkTimeout.Add(TimeSpan.FromSeconds(1)));

        // Assert
        Assert.True(disconnectEventRaised);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingValidInitMessage_Then_IsInitialized()
    {
        // Arrange
        var initMessage = _messageFactory.CreateInitMessage(_nodeOptions.Value);
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, true);

        // Act
        var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(peer, [peer, initMessage]);

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
        var pingMessage = _messageFactory.CreatePingMessage();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act
        var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(peer, [peer, pingMessage]);

        // Assert
        Assert.True(disconnectEventRaised);
    }

    [Fact]
    public void Given_InboundPeer_When_ReceivingIncompatibleFeatures_Then_Disconnects()
    {
        // Arrange
        var disconnectEventRaised = false;
        var features = _nodeOptions.Value.GetNodeFeatures();
        features.SetFeature(Feature.OPTION_ZEROCONF, true);
        var initMessage = new InitMessage(new InitPayload(features), _nodeOptions.Value.GetInitTlvs());

        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act
        var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(peer, [peer, initMessage]);

        // Assert
        Assert.True(disconnectEventRaised);
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
        var initMessage = _messageFactory.CreateInitMessage(otherNodeOptions);

        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, true);
        peer.DisconnectEvent += (_, _) => disconnectEventRaised = true;

        // Act
        var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(peer, [peer, initMessage]);

        // Assert
        Assert.True(disconnectEventRaised);
    }

    [Fact]
    public void Given_Peer_When_ReceivingPingMessage_Then_SendsPongMessage()
    {
        // Arrange
        const bool IS_INBOUND = false;
        var pingMessage = _messageFactory.CreatePingMessage();
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        _mockMessageService.Setup(m => m.SendMessageAsync(It.IsAny<PongMessage>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, IS_INBOUND);

        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        field.SetValue(peer, true);

        // Act
        var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(peer, [peer, pingMessage]);

        // Assert
        _mockMessageService.Verify();
    }

    [Fact]
    public void Given_Peer_When_ReceivingPongMessage_Then_PingPongServiceHandlesPong()
    {
        // Arrange
        var pongMessage = _messageFactory.CreatePongMessage(new PingMessage(new PingPayload { NumPongBytes = 1 }));
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);

        var peer = new Peer(_mockMessageService.Object, _mockPingPongService.Object, _messageFactory,
                            _mockLogger.Object, _nodeOptions, _peerAddress, false);

        var field = peer.GetType().GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
        field.SetValue(peer, true);

        // Act
        var method = peer.GetType().GetMethod("HandleMessage", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        method.Invoke(peer, [peer, pongMessage]);

        // Assert
        _mockPingPongService.Verify(p => p.HandlePong(It.IsAny<PongMessage>()), Times.Once());
    }
}