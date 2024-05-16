using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_ack_rbf message.
/// </summary>
/// <remarks>
/// The tx_ack_rbf message acknowledges the replacement of the transaction.
/// The message type is 73.
/// </remarks>
/// <param name="payload">The tx_ack_rbf payload.</param>
/// <param name="extension">The TLV extension.</param>
public sealed class TxAckRbfMessage(TxAckRbfPayload payload, TlvStream extension) : BaseMessage(MessageTypes.TX_ACK_RBF, payload, extension)
{
    /// <inheritdoc/>
    public new TxAckRbfPayload Payload { get => (TxAckRbfPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxAckRbfMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxAckRbfMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxAckRbfMessage</exception>
    public static async Task<TxAckRbfMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxAckRbfPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream) ?? throw new SerializationException("Required extension is missing");

            return new TxAckRbfMessage(payload, extension);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxAckRbfMessage", e);
        }
    }
}