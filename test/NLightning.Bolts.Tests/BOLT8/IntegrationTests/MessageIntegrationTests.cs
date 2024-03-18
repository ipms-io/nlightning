using System.Text;
using NLightning.Bolts.BOLT8.Constants;
using NLightning.Bolts.Tests.BOLT8.Utils;

namespace NLightning.Bolts.Tests.BOLT8.IntegrationTests;

public partial class MessageIntegrationTests
{
    [Fact]
    public void Given_TwoParties_When_MessageIsSent_Then_MessageIsReceived()
    {
        // Arrange
        var initializedParties = new InitializedPartiesUtil();
        var validMessages = new ValidMessagesUtil();

        // Make sure keys match
        Assert.Equal(validMessages.InitiatorSk, initializedParties.InitiatorSk);
        Assert.Equal(validMessages.InitiatorRk, initializedParties.InitiatorRk);

        var message = Encoding.ASCII.GetBytes("hello");
        var messageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
        var receivedMessageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

        for (var i = 0; i < 1002; i++)
        {
            // Act
            var messageSize = initializedParties.InitiatorTransport.WriteMessage(message, messageBuffer);
            var receivedMessageLenght = initializedParties.ResponderTransport.ReadMessageLength(messageBuffer.AsSpan(0, 18));

            Assert.Equal(18 + receivedMessageLenght, messageSize);

            var receivedMessageSize = initializedParties.ResponderTransport.ReadMessagePayload(messageBuffer.AsSpan(18, receivedMessageLenght), receivedMessageBuffer);

            // Assert
            Assert.Equal(message, receivedMessageBuffer[..receivedMessageSize]);

            switch (i)
            {
                case 0:
                    Assert.Equal(validMessages.Message0, messageBuffer[..messageSize]);
                    break;
                case 1:
                    Assert.Equal(validMessages.Message1, messageBuffer[..messageSize]);
                    break;
                case 500:
                    Assert.Equal(validMessages.Message500, messageBuffer[..messageSize]);
                    break;
                case 501:
                    Assert.Equal(validMessages.Message501, messageBuffer[..messageSize]);
                    break;
                case 1000:
                    Assert.Equal(validMessages.Message1000, messageBuffer[..messageSize]);
                    break;
                case 1001:
                    Assert.Equal(validMessages.Message1001, messageBuffer[..messageSize]);
                    break;
                default:
                    break;
            }
        }
    }
}