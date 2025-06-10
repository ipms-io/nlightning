using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;
using NLightning.Application.Node.Interfaces;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Infrastructure.Tests.Node.Managers;

using Domain.Exceptions;
using Domain.Node.Options;
using Infrastructure.Node.Managers;
using Infrastructure.Protocol.Models;
using NLightning.Tests.Utils;

// ReSharper disable AccessToDisposedClosure
public class PeerManagerTests
{
    private readonly CompactPubKey _compactPubKey =
        new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7").ToBytes();

    private readonly Mock<ILogger<PeerManager>> _mockLogger = new();
    private readonly Mock<IPeerServiceFactory> _mockPeerServiceFactory = new();
    private readonly Mock<IPeerService> _mockPeerService = new();
    private static readonly NodeOptions s_nodeOptions = new();
    private readonly IOptions<NodeOptions> _nodeOptionsWrapper = new OptionsWrapper<NodeOptions>(s_nodeOptions);

    public PeerManagerTests()
    {
        // Set up the mock peer service
        _mockPeerService.SetupGet(p => p.PeerPubKey).Returns(_compactPubKey);

        // Set up the peer service factory to return our mock peer service
        _mockPeerServiceFactory
           .Setup(f => f.CreateConnectedPeerAsync(It.IsAny<CompactPubKey>(), It.IsAny<TcpClient>()))
           .ReturnsAsync(_mockPeerService.Object);

        _mockPeerServiceFactory
           .Setup(f => f.CreateConnectingPeerAsync(It.IsAny<TcpClient>()))
           .ReturnsAsync(_mockPeerService.Object);
    }

    [Fact]
    public async Task Given_ValidPeerAddress_When_ConnectToPeerAsync_IsCalled_Then_PeerIsAdded()
    {
        // Given
        var availablePort = await PortPoolUtil.GetAvailablePortAsync();
        var tcpListener = new TcpListener(IPAddress.Loopback, availablePort);
        tcpListener.Start();

        try
        {
            var peerManager = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerServiceFactory.Object);
            var peerAddress = new PeerAddress(_compactPubKey, tcpListener.LocalEndpoint.ToEndpointString());
            var acceptTask = Task.Run(async () =>
            {
                _ = await tcpListener.AcceptTcpClientAsync();
            });

            // When
            await peerManager.ConnectToPeerAsync(peerAddress);
            await acceptTask;

            // Then
            var field = peerManager.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
            var peers = field?.GetValue(peerManager);
            Assert.NotNull(peers);
            Assert.True(peers is Dictionary<CompactPubKey, IPeerService>);
            Assert.True(((Dictionary<CompactPubKey, IPeerService>)peers).ContainsKey(peerAddress.PubKey));

            // Verify disconnect event was set up
            // _mockPeerService.Verify(p => p.DisconnectEvent, Times.Once);
        }
        finally
        {
            tcpListener.Dispose();
            PortPoolUtil.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Given_ConnectionError_When_ConnectToPeerAsync_IsCalled_Then_ThrowException()
    {
        var availablePort = await PortPoolUtil.GetAvailablePortAsync();

        try
        {
            // Given
            var peerManager = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerServiceFactory.Object);
            var peerAddress = new PeerAddress(_compactPubKey, IPAddress.Loopback.ToString(), availablePort);

            // When & Then
            var exception = await Assert
                               .ThrowsAnyAsync<ConnectionException>(() => peerManager.ConnectToPeerAsync(peerAddress));
            Assert.Equal($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", exception.Message);
        }
        finally
        {
            PortPoolUtil.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Given_ValidTcpClient_When_AcceptPeerAsync_IsCalled_Then_PeerIsAdded()
    {
        // Given
        var peerManager = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerServiceFactory.Object);
        var tcpClient = new TcpClient();

        // When
        await peerManager.AcceptPeerAsync(tcpClient);

        // Then
        var field = peerManager.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
        var peers = field?.GetValue(peerManager);
        Assert.NotNull(peers);
        Assert.True(peers is Dictionary<CompactPubKey, IPeerService>);
        Assert.True(((Dictionary<CompactPubKey, IPeerService>)peers).ContainsKey(_compactPubKey));

        // Verify disconnect event was set up
        // _mockPeerService.Verify(p => p.DisconnectEvent += It.IsAny<EventHandler>(), Times.Once);
    }

    [Fact]
    public void Given_ExistingPeer_When_DisconnectPeer_IsCalled_Then_PeerIsDisconnected()
    {
        // Given
        var peerManager = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerServiceFactory.Object);
        var field = peerManager.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
        var peers = (Dictionary<CompactPubKey, IPeerService>)field!.GetValue(peerManager)!;
        peers.Add(_compactPubKey, _mockPeerService.Object);

        // When
        peerManager.DisconnectPeer(_compactPubKey);

        // Then
        _mockPeerService.Verify(p => p.Disconnect(), Times.Once);
    }

    [Fact]
    public void Given_NonExistingPeer_When_DisconnectPeer_IsCalled_Then_LogWarning()
    {
        // Given
        var peerManager = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerServiceFactory.Object);

        // When
        peerManager.DisconnectPeer(_compactPubKey);

        // Then
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Peer") && o.ToString()!.Contains("not found")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
// ReSharper restore AccessToDisposedClosure