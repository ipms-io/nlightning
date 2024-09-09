using System.Net;
using System.Net.Sockets;
using System.Reflection;
using NBitcoin;

namespace NLightning.Bolts.Tests.BOLT1.Services;

using Bolts.BOLT1.Interfaces;
using Bolts.BOLT1.Primitives;
using Bolts.BOLT1.Services;
using Bolts.BOLT8.Interfaces;
using Common.Exceptions;
using Exceptions;
using Fixtures;
using Mock;

public class PeerServiceTests
{
    private readonly PubKey _pubKey = new("028d7500dd4c12685d1f568b4c2b5048e8534b873319f3a8daa612b469132ec7f7");
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
        var availablePort = await PortPool.GetAvailablePortAsync();
        var tcpListener = new TcpListener(IPAddress.Loopback, availablePort);
        tcpListener.Start();

        try
        {
            _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                        .Returns(_mockTransportService.Object);
            _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                      .Returns(_mockMessageService.Object);
            _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                       .Returns(_mockPingPongService.Object);

            var peerService = new PeerService(_nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);

            var peerAddress = new PeerAddress(_pubKey, tcpListener.LocalEndpoint.ToEndpointString());

            _ = Task.Run(async () =>
            {
                _ = await tcpListener.AcceptTcpClientAsync();
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
        finally
        {
            tcpListener.Dispose();
            PortPool.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Given_ConnectionError_When_ConnectToPeerAsync_IsCalled_Then_ThrowException()
    {
        // Arrange
        var availablePort = await PortPool.GetAvailablePortAsync();

        try
        {
            _mockTransportServiceFactory.Setup(f => f.CreateTransportService(It.IsAny<bool>(), It.IsAny<byte[]>(), It.IsAny<byte[]>(), It.IsAny<TcpClient>()))
                                        .Returns(_mockTransportService.Object);
            _mockMessageServiceFactory.Setup(f => f.CreateMessageService(It.IsAny<ITransportService>()))
                                      .Returns(_mockMessageService.Object);
            _mockPingPongServiceFactory.Setup(f => f.CreatePingPongService(It.IsAny<TimeSpan>()))
                                       .Returns(_mockPingPongService.Object);

            var peerService = new PeerService(_nodeOptions, _mockTransportServiceFactory.Object, _mockPingPongServiceFactory.Object, _mockMessageServiceFactory.Object);

            var peerAddress = new PeerAddress(_pubKey, IPAddress.Loopback.ToString(), availablePort);

            // Act & Assert
            var exception = await Assert.ThrowsAnyAsync<ConnectionException>(() => peerService.ConnectToPeerAsync(peerAddress));
            Assert.Equal($"Failed to connect to peer {peerAddress.Host}:{peerAddress.Port}", exception.Message);
        }
        finally
        {
            PortPool.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Given_ValidTcpClient_When_AcceptPeerAsync_IsCalled_Then_PeerIsAdded()
    {
        // Arrange
        var availablePort = await PortPool.GetAvailablePortAsync();
        var tcpListener = new TcpListener(IPAddress.Loopback, availablePort);
        tcpListener.Start();

        try
        {
            var tsc = new TaskCompletionSource<bool>();
            var pubkey = new Key().PubKey;

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
        finally
        {
            tcpListener.Dispose();
            PortPool.ReleasePort(availablePort);
        }
    }

    [Fact]
    public async Task Given_InvalidRemotePublicKey_When_AcceptPeerAsync_IsCalled_Then_ThrowException()
    {
        // Arrange
        var availablePort = await PortPool.GetAvailablePortAsync();
        var tcpListener = new TcpListener(IPAddress.Loopback, availablePort);
        tcpListener.Start();

        try
        {
            var tsc = new TaskCompletionSource<bool>();

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
            var exception = await Assert.ThrowsAnyAsync<ErrorException>(() => peerService.AcceptPeerAsync(tcpClient!));
            Assert.Equal("Failed to get remote static public key", exception.Message);
        }
        finally
        {
            tcpListener.Dispose();
            PortPool.ReleasePort(availablePort);
        }
    }
}