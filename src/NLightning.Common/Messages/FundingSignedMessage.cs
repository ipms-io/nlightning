using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a funding_signed message.
/// </summary>
/// <remarks>
/// The funding_signed message is sent by the funder to the fundee after the funding transaction has been created.
/// The message type is 35.
/// </remarks>
public sealed class FundingSignedMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new FundingSignedPayload Payload { get => (FundingSignedPayload)base.Payload; }

    public FundingSignedMessage(FundingSignedPayload payload) : base(MessageTypes.ACCEPT_CHANNEL_2, payload)
    { }

    /// <summary>
    /// Deserialize a FundingSignedMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized FundingSignedMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing FundingSignedMessage</exception>
    public static async Task<FundingSignedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await FundingSignedPayload.DeserializeAsync(stream);

            return new FundingSignedMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing FundingCreatedMessage", e);
        }
    }
}