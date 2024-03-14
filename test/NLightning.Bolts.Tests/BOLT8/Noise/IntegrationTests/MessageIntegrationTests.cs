using System.Text;
using NLightning.Bolts.BOLT8.Noise.Primitives;
using NLightning.Bolts.Tests.BOLT8.Noise.Utils;

namespace NLightning.Bolts.Tests.BOLT8.Noise.IntegrationTests;

public partial class MessageIntegrationTests
{
    [Fact]
    public void Given_TwoParties_When_MessageIsSent_Then_MessageIsReceived()
    {
        // Arrange
        var _initializedParties = new InitializedPartiesUtil();
        var _validMessages = new ValidMessagesUtil();

        // Make sure keys match
        Assert.Equal(_validMessages.InitiatorSk, _initializedParties.InitiatorSk);
        Assert.Equal(_validMessages.InitiatorRk, _initializedParties.InitiatorRk);

        var message = Encoding.ASCII.GetBytes("hello");
        var messageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];
        var receivedMessageBuffer = new byte[Protocol.MAX_MESSAGE_LENGTH];

        for (var i = 0; i < 1002; i++)
        {
            // Act
            var messageSize = _initializedParties.InitiatorTransport.WriteMessage(message, messageBuffer);
            var receivedMessageLenght = _initializedParties.ResponderTransport.ReadMessageLength(messageBuffer.AsSpan(0, 18));

            Assert.Equal(18 + receivedMessageLenght, messageSize);

            var receivedMessageSize = _initializedParties.ResponderTransport.ReadMessagePayload(messageBuffer.AsSpan(18, receivedMessageLenght), receivedMessageBuffer);

            // Assert
            Assert.Equal(message, receivedMessageBuffer[..receivedMessageSize]);

            switch (i)
            {
                case 0:
                    Assert.Equal(_validMessages.Message0, messageBuffer[..messageSize]);
                    break;
                case 1:
                    Assert.Equal(_validMessages.Message1, messageBuffer[..messageSize]);
                    break;
                case 500:
                    Assert.Equal(_validMessages.Message500, messageBuffer[..messageSize]);
                    break;
                case 501:
                    Assert.Equal(_validMessages.Message501, messageBuffer[..messageSize]);
                    break;
                case 1000:
                    Assert.Equal(_validMessages.Message1000, messageBuffer[..messageSize]);
                    break;
                case 1001:
                    Assert.Equal(_validMessages.Message1001, messageBuffer[..messageSize]);
                    break;
                default:
                    break;
            }
        }
    }
}