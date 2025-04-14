using System.Net;
using System.Net.Sockets;
using NBitcoin;
using NLightning.Bolts.BOLT8.Interfaces;
using NLightning.Common.Interfaces;

namespace NLightning.Bolts.Tests.BOLT8.Services;

using BOLT1.Fixtures;
using Bolts.BOLT8.Services;
using Common.Exceptions;
using Common.Managers;
using Mock;
using TestCollections;
using Tests.Utils;

// ReSharper disable AccessToDisposedClosure
[Collection(ConfigManagerCollection.NAME)]
public class TransportServiceTests
{
    private readonly Mock<ILogger> _mockLogger = new();

    [Fact]
    public async Task Given_TransportServiceAsInitiator_When_InitializeIsCalled_Then_HandshakeServicePerformStepIsCalledTwice()
    {
        // Arrange
        ConfigManager.Instance.NetworkTimeout = TimeSpan.FromSeconds(30);
        var availablePort = await PortPool.GetAvailablePortAsync();
        var tcpListener = new TcpListener(IPAddress.Loopback, availablePort);
        tcpListener.Start();

        try
        {
            var handshakeServiceMock = new Mock<FakeHandshakeService>();
            var steps = 2;
            handshakeServiceMock.Setup(x => x.PerformStep(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny)).Returns((byte[] inMessage, out byte[] outMessage) =>
            {
                ITransport? transport = null;
                switch (steps)
                {
                    case 2:
                        {
                            steps--;
                            if (inMessage.Length != 50 && inMessage.Length != 0)
                            {
                                throw new InvalidOperationException("Expected 50 bytes");
                            }
                            outMessage = new byte[50];
                            return (50, transport);
                        }
                    case 1:
                        {
                            steps--;
                            if (inMessage.Length != 50 && inMessage.Length != 66 && inMessage.Length != 0)
                            {
                                throw new InvalidOperationException("Expected 66 bytes");
                            }
                            outMessage = new byte[66];

                            return (66, new FakeTransport());
                        }
                    default:
                        throw new InvalidOperationException("There's no more steps to complete");
                }
            });
            handshakeServiceMock.Object.SetIsInitiator(true);
            var tcpClient1 = new TcpClient();
            var acceptTask = Task.Run(async () =>
            {
                var tcpClient2 = await tcpListener.AcceptTcpClientAsync();
                var stream = tcpClient2.GetStream();
                var buffer = new byte[50];
                await stream.ReadExactlyAsync(buffer);

                await stream.WriteAsync(buffer);

                buffer = new byte[66];
                await stream.ReadExactlyAsync(buffer);
            });
            await tcpClient1.ConnectAsync(IPEndPoint.Parse(tcpListener.LocalEndpoint.ToEndpointString()));
            var transportService = new TransportService(_mockLogger.Object, handshakeServiceMock.Object, tcpClient1);

            // Act
            await transportService.InitializeAsync();
            await acceptTask;

            // Assert
            handshakeServiceMock.Verify(x => x.PerformStep(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny), Times.Exactly(2));
        }
        finally
        {
            tcpListener.Dispose();
            PortPool.ReleasePort(availablePort);

            ConfigManagerUtil.ResetConfigManager();
        }
    }

    [Fact]
    public async Task Given_TransportServiceAsInitiator_When_InitializeIsCalledAndTcpClinetIsDisconnected_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        ConfigManager.Instance.NetworkTimeout = TimeSpan.FromSeconds(30);
        var handshakeServiceMock = new Mock<FakeHandshakeService>();
        var tcpClient1 = new TcpClient();
        var transportService = new TransportService(_mockLogger.Object, handshakeServiceMock.Object, tcpClient1);

        // Act
        var exception = await Assert.ThrowsAnyAsync<InvalidOperationException>(() => transportService.InitializeAsync());

        // Assert
        Assert.Equal("TcpClient is not connected", exception.Message);

        ConfigManagerUtil.ResetConfigManager();
    }

    [Fact]
    public async Task Given_TransportService_When_TimeoutOccurs_Then_ThrowsConnectionTimeoutException()
    {
        // Arrange
        ConfigManager.Instance.NetworkTimeout = TimeSpan.Zero;
        var availablePort = await PortPool.GetAvailablePortAsync();
        var tcpListener = new TcpListener(IPAddress.Loopback, availablePort);
        tcpListener.Start();

        try
        {
            var handshakeServiceMock = new Mock<FakeHandshakeService>();
            var steps = 2;
            handshakeServiceMock.Setup(x => x.PerformStep(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny)).Returns((byte[] inMessage, out byte[] outMessage) =>
            {
                if (steps != 2)
                {
                    throw new InvalidOperationException("There's no more steps to complete");
                }

                steps--;
                if (inMessage.Length != 50 && inMessage.Length != 0)
                {
                    throw new InvalidOperationException("Expected 50 bytes");
                }
                outMessage = new byte[50];
                ITransport? transport = null;
                return (50, transport);

            });
            handshakeServiceMock.Object.SetIsInitiator(true);
            var tcpClient1 = new TcpClient();
            var acceptTask = Task.Run(async () =>
            {
                var tcpClient2 = await tcpListener.AcceptTcpClientAsync();
                var stream = tcpClient2.GetStream();
                var buffer = new byte[50];
                await stream.ReadExactlyAsync(buffer);

                await stream.WriteAsync(buffer);
            });
            await tcpClient1.ConnectAsync(IPEndPoint.Parse(tcpListener.LocalEndpoint.ToEndpointString()));
            var transportService = new TransportService(_mockLogger.Object, handshakeServiceMock.Object, tcpClient1);

            // Act
            var exception = await Assert.ThrowsAnyAsync<ConnectionTimeoutException>(() => transportService.InitializeAsync());
            await acceptTask;

            // Assert
            Assert.Equal("Timeout while reading Handhsake's Act 2", exception.Message);
        }
        finally
        {
            tcpListener.Dispose();
            PortPool.ReleasePort(availablePort);

            ConfigManagerUtil.ResetConfigManager();
        }
    }
}
// ReSharper restore AccessToDisposedClosure