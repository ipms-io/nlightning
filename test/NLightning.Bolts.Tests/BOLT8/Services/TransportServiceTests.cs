using System.Net;
using System.Net.Sockets;

namespace NLightning.Bolts.Tests.BOLT8.Services;

using Bolts.BOLT8.Services;
using Mock;
using NBitcoin;

public partial class TransportServiceTests
{
    [Fact]
    public async void Given_TransportServiceAsInitiator_When_InitializeIsCalled_Then_HandshakeServicePerformStepIsCalledTwice()
    {
        // Arrange
        var handshakeServiceMock = new Mock<FakeHandshakeService>();
        var steps = 2;
        handshakeServiceMock.Setup(x => x.PerformStep(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny)).Returns((byte[] inMessage, out byte[] outMessage) =>
        {
            if (steps == 2)
            {
                steps--;
                if (inMessage.Length != 50 && inMessage.Length != 0)
                {
                    throw new InvalidOperationException("Expected 50 bytes");
                }
                outMessage = new byte[50];
                return 50;
            }
            else if (steps == 1)
            {
                steps--;
                if (inMessage.Length != 50 && inMessage.Length != 66 && inMessage.Length != 0)
                {
                    throw new InvalidOperationException("Expected 66 bytes");
                }
                outMessage = new byte[66];

                handshakeServiceMock.Object.SetTransport(new FakeTransport());

                return 66;
            }

            throw new InvalidOperationException("There's no more steps to complete");
        });
        handshakeServiceMock.Object.SetIsInitiator(true);
        var tcpClient1 = new TcpClient();
        var tcpListener = new TcpListener(IPAddress.Loopback, 65535);
        tcpListener.Start();
        _ = Task.Run(async () =>
        {
            var tcpClient2 = await tcpListener.AcceptTcpClientAsync();
            var stream = tcpClient2.GetStream();
            var buffer = new byte[50];
            await stream.ReadExactlyAsync(buffer);

            await stream.WriteAsync(buffer);

            buffer = new byte[66];
            await stream.ReadExactlyAsync(buffer);
        });
        tcpClient1.Connect(IPEndPoint.Parse(tcpListener.LocalEndpoint.ToEndpointString()));
        var transportService = new TransportService(handshakeServiceMock.Object, tcpClient1);

        // Act
        await transportService.Initialize();

        // Assert
        handshakeServiceMock.Verify(x => x.PerformStep(It.IsAny<byte[]>(), out It.Ref<byte[]>.IsAny), Times.Exactly(2));
    }

    [Fact]
    public void Given_TransportServiceAsInitiator_When_InitializeIsCalledAndTcpClinetIsDisconnected_Then_ThrowsInvalidOperationException()
    {
        // Arrange
        var handshakeServiceMock = new Mock<FakeHandshakeService>();
        var tcpClient1 = new TcpClient();
        var transportService = new TransportService(handshakeServiceMock.Object, tcpClient1);

        // Act
        var exception = Assert.ThrowsAnyAsync<InvalidOperationException>(() => transportService.Initialize());

        // Assert
        Assert.Equal("TcpClient is not connected", exception.Result.Message);
    }
}