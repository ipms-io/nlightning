using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a funding_created message.
/// </summary>
/// <remarks>
/// The funding_created message is sent by the funder to the fundee after the funding transaction has been created.
/// The message type is 34.
/// </remarks>
public sealed class FundingCreatedMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new FundingCreatedPayload Payload { get => (FundingCreatedPayload)base.Payload; }

    public FundingCreatedMessage(FundingCreatedPayload payload) : base(MessageTypes.ACCEPT_CHANNEL_2, payload)
    { }

    /// <summary>
    /// Deserialize a FundingCreatedMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized FundingCreatedMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing FundingCreatedMessage</exception>
    public static async Task<FundingCreatedMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await FundingCreatedPayload.DeserializeAsync(stream);

            return new FundingCreatedMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing FundingCreatedMessage", e);
        }
    }
}