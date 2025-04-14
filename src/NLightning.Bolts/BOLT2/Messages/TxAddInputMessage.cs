using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Payloads;

/// <summary>
/// Represents an tx_add_input message.
/// </summary>
/// <remarks>
/// The tx_add_input message is used to add an input to the transaction.
/// The message type is 66.
/// </remarks>
/// <param name="payload">The tx_add_input payload.</param>
public sealed class TxAddInputMessage(TxAddInputPayload payload) : BaseMessage(MessageTypes.TX_ADD_INPUT, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxAddInputPayload Payload { get => (TxAddInputPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxAddInputMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAddInputMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAddInputMessage</exception>
    public static async Task<TxAddInputMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxAddInputPayload.DeserializeAsync(stream);

            return new TxAddInputMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAddInputMessage", e);
        }
    }
}