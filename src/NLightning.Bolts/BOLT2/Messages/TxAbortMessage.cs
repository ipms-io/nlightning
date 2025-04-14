using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Payloads;

/// <summary>
/// Represents a tx_abort message.
/// </summary>
/// <remarks>
/// The tx_abort message allows for the cancellation of an in-progress negotiation.
/// The message type is 74.
/// </remarks>
/// <param name="payload">The tx_abort payload.</param>
public sealed class TxAbortMessage(TxAbortPayload payload) : BaseMessage(MessageTypes.TX_ABORT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAbortPayload Payload { get => (TxAbortPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxAbortMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAbortMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAbortMessage</exception>
    public static async Task<TxAbortMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxAbortPayload.DeserializeAsync(stream);

            return new TxAbortMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAbortMessage", e);
        }
    }
}