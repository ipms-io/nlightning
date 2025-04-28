using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_add_output message.
/// </summary>
/// <remarks>
/// The tx_add_output message is used to add an output to the transaction.
/// The message type is 67.
/// </remarks>
/// <param name="payload">The tx_add_output payload.</param>
public sealed class TxAddOutputMessage(TxAddOutputPayload payload) : BaseMessage(MessageTypes.TX_ADD_OUTPUT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAddOutputPayload Payload { get => (TxAddOutputPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxAddOutputMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAddOutputMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAddOutputMessage</exception>
    public static async Task<TxAddOutputMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxAddOutputPayload.DeserializeAsync(stream);
            return new TxAddOutputMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAddOutputMessage", e);
        }
    }
}