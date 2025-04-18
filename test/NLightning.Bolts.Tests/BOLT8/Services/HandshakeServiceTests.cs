namespace NLightning.Bolts.Tests.BOLT8.Services;

using Bolts.BOLT8.Services;
using Mock;
using Utils;

public class HandshakeServiceTests
{
    private readonly Mock<FakeHandshakeState> _handshakeStateMock;

    public HandshakeServiceTests()
    {
        _handshakeStateMock = new Mock<FakeHandshakeState>();
        _handshakeStateMock.Setup(x => x.WriteMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns((1, null, null));
        _handshakeStateMock.Setup(x => x.ReadMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns((1, null, null));
    }

    #region Initiator Tests
    [Fact]
    public void Given_NewHandshakeServiceAsInitiator_When_PerformStepIsCalledByInitiator_Then_HandshakeStateWriteMessageIsCalled()
    {
        // Arrange
        var messageBuffer = new byte[100];
        var initiatorHandshakeService = new HandshakeService(true, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, _handshakeStateMock.Object);

        // Act
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Assert
        _handshakeStateMock.Verify(x => x.WriteMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void Given_HandshakeServiceAsInitiator_When_PerformStepIsCalledByInitiatorForTheSecondTime_Then_HandshakeStateWriteMessageIsCalled()
    {
        // Arrange
        var messageBuffer = new byte[100];
        var initiatorHandshakeService = new HandshakeService(true, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, _handshakeStateMock.Object);
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Act
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Assert
        _handshakeStateMock.Verify(x => x.WriteMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Exactly(2));
        _handshakeStateMock.Verify(x => x.ReadMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void Given_HandshakeServiceAsInitiator_When_PerformStepIsCalledByInitiatorForTheThirdTime_Then_ExceptionIsThrown()
    {
        // Arrange
        var messageBuffer = new byte[100];
        var initiatorHandshakeService = new HandshakeService(true, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, _handshakeStateMock.Object);
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _));

        // Assert
        Assert.Equal("There's no more steps to complete", exception.Message);
    }
    #endregion

    #region Responder Tests
    [Fact]
    public void Given_NewHandshakeServiceAsResponder_When_PerformStepIsCalledByInitiator_Then_HandshakeStateWriteMessageIsCalled()
    {
        // Arrange
        var messageBuffer = new byte[100];
        var initiatorHandshakeService = new HandshakeService(false, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, _handshakeStateMock.Object);

        // Act
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Assert
        _handshakeStateMock.Verify(x => x.ReadMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
        _handshakeStateMock.Verify(x => x.WriteMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void Given_HandshakeServiceAsResponder_When_PerformStepIsCalledByInitiatorForTheSecondTime_Then_HandshakeStateWriteMessageIsCalled()
    {
        // Arrange
        var messageBuffer = new byte[100];
        var initiatorHandshakeService = new HandshakeService(false, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, _handshakeStateMock.Object);
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Act
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Assert
        _handshakeStateMock.Verify(x => x.ReadMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Exactly(2));
        _handshakeStateMock.Verify(x => x.WriteMessageTest(It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Once);
    }

    [Fact]
    public void Given_HandshakeServiceAsResponder_When_PerformStepIsCalledByInitiatorForTheThirdTime_Then_ExceptionIsThrown()
    {
        // Arrange
        var messageBuffer = new byte[100];
        var initiatorHandshakeService = new HandshakeService(false, InitiatorValidKeysUtil.LocalStaticPrivateKey, InitiatorValidKeysUtil.RemoteStaticPublicKey, _handshakeStateMock.Object);
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);
        _ = initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _);

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => initiatorHandshakeService.PerformStep(messageBuffer, messageBuffer, out _));

        // Assert
        Assert.Equal("There's no more steps to complete", exception.Message);
    }
    #endregion
}