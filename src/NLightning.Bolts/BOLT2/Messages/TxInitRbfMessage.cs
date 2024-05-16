using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_init_rbf message.
/// </summary>
/// <remarks>
/// The tx_init_rbf message initiates a replacement of the transaction after it's been completed.
/// The message type is 72.
/// </remarks>
/// <param name="payload">The tx_init_rbf payload.</param>
/// <param name="extension">The TLV extension.</param>
public sealed class TxInitRbfMessage(TxInitRbfPayload payload, TLVStream extension) : BaseMessage(MessageTypes.TX_INIT_RBF, payload, extension)
{
    /// <inheritdoc/>
    public new TxInitRbfPayload Payload { get => (TxInitRbfPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxInitRbfMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxInitRbfMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxInitRbfMessage</exception>
    public static async Task<TxInitRbfMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Check message type
            await CheckMessageTypeAsync(stream, MessageTypes.TX_INIT_RBF);

            // Deserialize payload
            var payload = await TxInitRbfPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TLVStream.DeserializeAsync(stream) ?? throw new SerializationException("Required extension is missing");

            return new TxInitRbfMessage(payload, extension);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxInitRbfMessage", e);
        }
    }
}