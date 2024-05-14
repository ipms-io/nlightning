using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Payloads;

using Interfaces;

/// <summary>
/// Represents a tx_complete payload.
/// </summary>
/// <remarks>
/// The tx_complete payload is used to indicate that the transaction is complete.
/// </remarks>
/// <param name="channelId">The channel id.</param>
/// <seealso cref="Messages.TxCompleteMessage"/>
/// <seealso cref="Common.Types.ChannelId"/>
public class TxCompletePayload(ChannelId channelId) : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChannelId.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserializes a TxCompletePayload from a stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The deserialized TxCompletePayload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload.</exception>
    public static async Task<TxCompletePayload> DeserializeAsync(Stream stream)
    {
        try
        {
            var channelId = await ChannelId.DeserializeAsync(stream);
            return new TxCompletePayload(channelId);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing TxCompletePayload", e);
        }
    }
}