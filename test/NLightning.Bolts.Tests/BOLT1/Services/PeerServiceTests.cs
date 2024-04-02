using System.Net;
using System.Net.Sockets;
using System.Reflection;
using NBitcoin;
using NLightning.Bolts.BOLT1.Interfaces;
using NLightning.Bolts.BOLT1.Primitives;
using NLightning.Bolts.BOLT1.Services;
using NLightning.Bolts.BOLT8.Interfaces;
using NLightning.Bolts.Tests.BOLT1.Mock;

namespace NLightning.Bolts.Tests.BOLT1.Services;

public class PeerServiceTests
{
    private readonly Random _random = new();
    private readonly NodeOptions _nodeOptions = new();
    private readonly Mock<ITransportService> _mockTransportService = new();
    private readonly Mock<IMessageService> _mockMessageService = new();
    private readonly Mock<IPingPongService> _mockPingPongService = new();
    private readonly Mock<FakeTransportServiceFactory> _mockTransportServiceFactory = new();
    private readonly Mock<IMessageServiceFactory> _mockMessageServiceFactory = new();
    private readonly Mock<IPingPongServiceFactory> _mockPingPongServiceFactory = new();

    public PeerServiceTests()
    {
        _mockMessageService.SetupGet(m => m.IsConnected).Returns(true);
    }

    [Fact]
    public async Task Given_ValidPeerAddress_When_ConnectToPeerAsync_IsCalled_Then_PeerIsAdded()
    {
        // Arrange
        _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                    .Returns(_mockTransportService.Object);
        _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                  .Returns(_mockMessageService.Object);
        _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                   .Returns(_mockPingPongService.Object);

        var peerService = new PeerService(_nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);

        var tcpListener = new TcpListener(IPAddress.Loopback, _random.Next(1024, 65535));
        tcpListener.Start();

        var peerAddress = new PeerAddress(new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7"), tcpListener.LocalEndpoint.ToEndpointString());

        _ = Task.Run(async () =>
        {
            await tcpListener.AcceptTcpClientAsync();
        });

        // Act
        await peerService.ConnectToPeerAsync(peerAddress);

        // Assert
        var field = peerService.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
        var peers = field?.GetValue(peerService);
        Assert.NotNull(peers);
        Assert.True(peers is Dictionary<PubKey, Peer>);
        Assert.True(((Dictionary<PubKey, Peer>)peers).ContainsKey(peerAddress.PubKey));
    }

    [Fact]
    public async Task Given_ConnectionTimeout_When_ConnectToPeerAsync_IsCalled_Then_ThrowException()
    {
        // Arrange
        _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                    .Returns(_mockTransportService.Object);
        _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                  .Returns(_mockMessageService.Object);
        _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                   .Returns(_mockPingPongService.Object);

        var nodeOptions = new NodeOptions { NetworkTimeout = TimeSpan.FromMicroseconds(1) };

        var peerService = new PeerService(nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);

        var tcpListener = new TcpListener(IPAddress.Loopback, _random.Next(1024, 65535));
        // tcpListener.Start();

        var peerAddress = new PeerAddress(new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7"), tcpListener.LocalEndpoint.ToEndpointString());

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => peerService.ConnectToPeerAsync(peerAddress));
        Assert.Equal($"Timeout connecting to peer {peerAddress.Host}:{peerAddress.Port}", exception.Message);
    }

    [Fact]
    public async Task Given_ConnectionError_When_ConnectToPeerAsync_IsCalled_Then_ThrowException()
    {
        // Arrange
        _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                    .Returns(_mockTransportService.Object);
        _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                  .Returns(_mockMessageService.Object);
        _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                   .Returns(_mockPingPongService.Object);

        var nodeOptions = new NodeOptions { NetworkTimeout = TimeSpan.FromSeconds(1) };

        var peerService = new PeerService(_nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);

        var tcpListener = new TcpListener(IPAddress.Loopback, _random.Next(1024, 65535));

        var peerAddress = new PeerAddress(new PubKey("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7"), tcpListener.LocalEndpoint.ToEndpointString());

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => peerService.ConnectToPeerAsync(peerAddress));
        Assert.Equal($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", exception.Message);
    }

    [Fact]
    public async Task Given_ValidTcpClient_When_AcceptPeerAsync_IsCalled_Then_PeerIsAdded()
    {
        // Arrange
        var tsc = new TaskCompletionSource<bool>();
        var pubkey = new Key().PubKey;
        var tcpListener = new TcpListener(IPAddress.Loopback, _random.Next(1024, 65535));
        tcpListener.Start();

        _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                    .Returns(_mockTransportService.Object);
        _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                  .Returns(_mockMessageService.Object);
        _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                   .Returns(_mockPingPongService.Object);
        _mockTransportService.SetupGet(t => t.RemoteStaticPublicKey).Returns(pubkey);

        var peerService = new PeerService(_nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);
        TcpClient? tcpClient = null;
        _ = Task.Run(async () =>
        {
            tcpClient = await tcpListener.AcceptTcpClientAsync();
            tsc.SetResult(true);
        });
        var tcpClient2 = new TcpClient();
        tcpClient2.Connect(IPEndPoint.Parse(tcpListener.LocalEndpoint.ToString()!));

        await tsc.Task;

        // Act
        await peerService.AcceptPeerAsync(tcpClient!);

        // Assert
        var field = peerService.GetType().GetField("_peers", BindingFlags.NonPublic | BindingFlags.Instance);
        var peers = field?.GetValue(peerService);
        Assert.NotNull(peers);
        Assert.True(peers is Dictionary<PubKey, Peer>);
        Assert.True(((Dictionary<PubKey, Peer>)peers).ContainsKey(pubkey));
    }

    [Fact]
    public async Task Given_InvalidRemotePublicKey_When_AcceptPeerAsync_IsCalled_Then_ThrowException()
    {
        // Arrange
        var tsc = new TaskCompletionSource<bool>();
        var pubkey = new Key().PubKey;
        var tcpListener = new TcpListener(IPAddress.Loopback, _random.Next(1024, 65535));
        tcpListener.Start();

        _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                    .Returns(_mockTransportService.Object);
        _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                  .Returns(_mockMessageService.Object);
        _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                   .Returns(_mockPingPongService.Object);

        var peerService = new PeerService(_nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);
        TcpClient? tcpClient = null;
        _ = Task.Run(async () =>
        {
            tcpClient = await tcpListener.AcceptTcpClientAsync();
            tsc.SetResult(true);
        });
        var tcpClient2 = new TcpClient();
        tcpClient2.Connect(IPEndPoint.Parse(tcpListener.LocalEndpoint.ToString()!));

        await tsc.Task;

        // Act & Assert
        var exception = await Assert.ThrowsAnyAsync<Exception>(() => peerService.AcceptPeerAsync(tcpClient!));
        Assert.Equal("Failed to get remote static public key", exception.Message);
    }
}