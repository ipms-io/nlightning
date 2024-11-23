using System.Text;

namespace NLightning.Bolts.Tests.BOLT8.IntegrationTests;

using Bolts.BOLT8.Constants;
using Utils;

public class MessageIntegrationTests
{
    [Fact]
    public void Given_TwoParties_When_MessageIsSent_Then_MessageIsReceived()
    {
        // Arrange
        var initializedParties = new InitializedPartiesUtil();

        // Make sure keys match
        Assert.Equal(ValidMessagesUtil.InitiatorSk, initializedParties.InitiatorSk);
        Assert.Equal(ValidMessagesUtil.InitiatorRk, initializedParties.InitiatorRk);

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
                    Assert.Equal(ValidMessagesUtil.Message0, messageBuffer[..messageSize]);
                    break;
                case 1:
                    Assert.Equal(ValidMessagesUtil.Message1, messageBuffer[..messageSize]);
                    break;
                case 500:
                    Assert.Equal(ValidMessagesUtil.Message500, messageBuffer[..messageSize]);
                    break;
                case 501:
                    Assert.Equal(ValidMessagesUtil.Message501, messageBuffer[..messageSize]);
                    break;
                case 1000:
                    Assert.Equal(ValidMessagesUtil.Message1000, messageBuffer[..messageSize]);
                    break;
                case 1001:
                    Assert.Equal(ValidMessagesUtil.Message1001, messageBuffer[..messageSize]);
                    break;
            }
        }
    }
}