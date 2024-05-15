using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a tx_signatures message.
/// </summary>
/// <remarks>
/// The tx_signatures message signals the provision of transaction signatures.
/// The message type is 71.
/// </remarks>
/// <param name="payload">The tx_signatures payload.</param>
public sealed class TxSignaturesMessage(TxSignaturesPayload payload) : BaseMessage(MessageTypes.TX_SIGNATURES, payload)
{
    /// <inheritdoc/>
    public new TxSignaturesPayload Payload { get => (TxSignaturesPayload)base.Payload; }

    /// <summary>
    /// Deserialize a TxSignaturesMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized TxSignaturesMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing TxSignaturesMessage</exception>
    public static async Task<TxSignaturesMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await TxSignaturesPayload.DeserializeAsync(stream);
            return new TxSignaturesMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing TxSignaturesMessage", e);
        }
    }
}