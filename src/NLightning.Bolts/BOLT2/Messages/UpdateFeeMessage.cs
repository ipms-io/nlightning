using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Exceptions;
using Payloads;

/// <summary>
/// Represents a update_fee message.
/// </summary>
/// <remarks>
/// The update_fee message is sent by the node which is paying the Bitcoin fee.
/// The message type is 134.
/// </remarks>
/// <param name="payload"></param>
public sealed class UpdateFeeMessage(UpdateFeePayload payload) : BaseMessage(MessageTypes.UPDATE_FEE, payload)
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new UpdateFeePayload Payload { get => (UpdateFeePayload)base.Payload; }

    /// <summary>
    /// Deserialize a UpdateFeeMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized UpdateFeeMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing UpdateFeeMessage</exception>
    public static async Task<UpdateFeeMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await UpdateFeePayload.DeserializeAsync(stream);

            return new UpdateFeeMessage(payload);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing UpdateFeeMessage", e);
        }
    }
}