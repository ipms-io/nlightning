using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_complete message.
/// </summary>
/// <remarks>
/// The tx_complete message signals the conclusion of a peer's transaction contributions.
/// The message type is 70.
/// </remarks>
/// <param name="payload">The tx_complete payload.</param>
public sealed class TxCompleteMessage(TxCompletePayload payload) : BaseMessage(MessageTypes.TX_COMPLETE, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxCompletePayload Payload { get => (TxCompletePayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxCompleteMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxCompleteMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxCompleteMessage</exception>
    public static async Task<TxCompleteMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxCompletePayload.DeserializeAsync(stream);
            return new TxCompleteMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxCompleteMessage", e);
        }
    }
}