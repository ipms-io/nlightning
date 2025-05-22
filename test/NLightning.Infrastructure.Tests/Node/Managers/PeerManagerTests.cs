using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBitcoin;

namespace NLightning.Infrastructure.Tests.Node.Managers;

using Domain.Exceptions;
using Domain.Node.Options;
using Domain.Protocol.Services;
using Infrastructure.Node.Managers;
using Infrastructure.Protocol.Models;
using NLightning.Tests.Utils;

// ReSharper disable AccessToDisposedClosure
public class PeerManagerTests
{
    private readonly PubKey _pubKey = new("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7");
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IPingPongService> _mockPingPongService = new();
    private readonly Mock<ILogger<PeerManager>> _mockLogger = new();
    private readonly Mock<ILogger<Peer>> _mockPeerLogger = new();
    private readonly Mock<IPeerFactory> _mockPeerFactory = new();
    private readonly Mock<IMessageFactory> _mockMessageFactory = new();
    private static readonly NodeOptions s_nodeOptions = new();
    private readonly IOptions<NodeOptions> _nodeOptionsWrapper = new OptionsWrapper<NodeOptions>(s_nodeOptions);

    public PeerManagerTests()
    {
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
        _mockPeerFactory.Setup(f => f.CreateConnectedPeerAsync(It.IsAny<PeerAddress>(), It.IsAny<TcpClient>()))
            .ReturnsAsync((PeerAddress peerAddress, TcpClient _) =>
                new Peer(s_nodeOptions.Features, _mockPeerLogger.Object, _mockMessageFactory.Object,
                         _mockMessageService.Object, s_nodeOptions.NetworkTimeout, peerAddress,
                         _mockPingPongService.Object));
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
            var peerService = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerFactory.Object);
            var peerAddress = new PeerAddress(_pubKey, tcpListener.LocalEndpoint.ToEndpointString());
            var acceptTask = Task.Run(async () =>
            {
                _ = await tcpListener.AcceptTcpClientAsync();
            });

            // When
            await peerService.ConnectToPeerAsync(peerAddress);
            await acceptTask;

            // Then
            var field = peerService.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
            var peers = field?.GetValue(peerService);
            Assert.NotNull(peers);
            Assert.True(peers is Dictionary<PubKey, Peer>);
            Assert.True(((Dictionary<PubKey, Peer>)peers).ContainsKey(peerAddress.PubKey));
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
            var peerManager = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerFactory.Object);
            var peerAddress = new PeerAddress(_pubKey, IPAddress.Loopback.ToString(), availablePort);

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
        var pubkey = new Key().PubKey;

        _mockPeerFactory.Setup(f => f.CreateConnectingPeerAsync(It.IsAny<TcpClient>()))
                       .ReturnsAsync((TcpClient _) => new Peer(s_nodeOptions.Features, _mockPeerLogger.Object,
                                                               _mockMessageFactory.Object, _mockMessageService.Object,
                                                               s_nodeOptions.NetworkTimeout,
                                                               new PeerAddress(pubkey, "127.0.0.1:9735"),
                                                               _mockPingPongService.Object));

        var peerService = new PeerManager(_mockLogger.Object, _nodeOptionsWrapper, _mockPeerFactory.Object);
        var tcpClient = new TcpClient();

        // When
        await peerService.AcceptPeerAsync(tcpClient);

        // Then
        var field = peerService.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
        var peers = field?.GetValue(peerService);
        Assert.NotNull(peers);
        Assert.True(peers is Dictionary<PubKey, Peer>);
        Assert.True(((Dictionary<PubKey, Peer>)peers).ContainsKey(pubkey));

    }
}
// ReSharper restore AccessToDisposedClosure