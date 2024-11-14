using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_remove_output message.
/// </summary>
/// <remarks>
/// The tx_remove_output message is used to remove an output from the transaction.
/// The message type is 69.
/// </remarks>
/// <param name="payload">The tx_remove_output payload.</param>
public sealed class TxRemoveOutputMessage(TxRemoveOutputPayload payload) : BaseMessage(MessageTypes.TX_REMOVE_OUTPUT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxRemoveOutputPayload Payload { get => (TxRemoveOutputPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxRemoveOutputMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxRemoveOutputMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxRemoveOutputMessage</exception>
    public static async Task<TxRemoveOutputMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxRemoveOutputPayload.DeserializeAsync(stream);
            return new TxRemoveOutputMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxRemoveOutputMessage", e);
        }
    }
}