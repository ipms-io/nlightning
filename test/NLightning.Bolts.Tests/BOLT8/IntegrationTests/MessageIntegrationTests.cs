namespace NLightning.Bolts.Tests.BOLT8.IntegrationTests;

using Common.Constants;
using Utils;

[Collection(TestCollections.NetworkCollection.NAME)]
public class MessageIntegrationTests
{
    [Fact]
    public void Given_TwoParties_When_MessageIsSent_Then_MessageIsReceived()
    {
        // Arrange
        var initializedParties = new InitializedPartiesUtil();

        try
        {
            // Make sure keys match
            Assert.Equal(((Span<byte>)ValidMessagesUtil.InitiatorSk).ToArray(),
                ((Span<byte>)initializedParties.InitiatorSk).ToArray());
            Assert.Equal(((Span<byte>)ValidMessagesUtil.InitiatorRk).ToArray(),
                ((Span<byte>)initializedParties.InitiatorRk).ToArray());

            var message = "hello"u8.ToArray();
            var messageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];
            var receivedMessageBuffer = new byte[ProtocolConstants.MAX_MESSAGE_LENGTH];

            for (var i = 0; i < 1002; i++)
            {
                // Act
                var messageSize = initializedParties.InitiatorTransport.WriteMessage(message, messageBuffer);
                var receivedMessageLength =
                    initializedParties.ResponderTransport.ReadMessageLength(messageBuffer.AsSpan(0, 18));

                Assert.Equal(18 + receivedMessageLength, messageSize);

                var receivedMessageSize =
                    initializedParties.ResponderTransport.ReadMessagePayload(
                        messageBuffer.AsSpan(18, receivedMessageLength), receivedMessageBuffer);

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
        finally
        {
            initializedParties.InitiatorTransport.Dispose();
            initializedParties.ResponderTransport.Dispose();
        }
    }
}