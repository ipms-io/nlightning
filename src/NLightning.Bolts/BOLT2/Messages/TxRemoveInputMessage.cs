using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_remove_input message.
/// </summary>
/// <remarks>
/// The tx_remove_input message is used to remove an input from the transaction.
/// The message type is 68.
/// </remarks>
/// <param name="payload">The tx_remove_input payload.</param>
public sealed class TxRemoveInputMessage(TxRemoveInputPayload payload) : BaseMessage(MessageTypes.TX_REMOVE_INPUT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxRemoveInputPayload Payload { get => (TxRemoveInputPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxRemoveInputMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxRemoveInputMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxRemoveInputMessage</exception>
    public static async Task<TxRemoveInputMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxRemoveInputPayload.DeserializeAsync(stream);
            return new TxRemoveInputMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxRemoveInputMessage", e);
        }
    }
}