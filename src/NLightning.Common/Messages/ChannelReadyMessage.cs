using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;
using TLVs;
using Types;

/// <summary>
/// Represents a channel_ready message.
/// </summary>
/// <remarks>
/// The channel_ready message indicates that the funding transaction has sufficient confirms for channel use.
/// The message type is 36.
/// </remarks>
public sealed class ChannelReadyMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ChannelReadyPayload Payload { get => (ChannelReadyPayload)base.Payload; }

    public ShortChannelIdTlv? ShortChannelIdTlv { get; }

    public ChannelReadyMessage(ChannelReadyPayload payload, ShortChannelIdTlv? shortChannelIdTlv = null)
        : base(MessageTypes.TX_ACK_RBF, payload)
    {
        ShortChannelIdTlv = shortChannelIdTlv;

        if (ShortChannelIdTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(ShortChannelIdTlv);
        }
    }

    /// <summary>
    /// Deserialize a ChannelReadyMessage from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized ChannelReadyMessage.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing ChannelReadyMessage</exception>
    public static async Task<ChannelReadyMessage> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await ChannelReadyPayload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new ChannelReadyMessage(payload);
            }

            var shortChannelIdTlv = extension.TryGetTlv(TlvConstants.SHORT_CHANNEL_ID, out var tlv)
                ? ShortChannelIdTlv.FromTlv(tlv!)
                : null;

            return new ChannelReadyMessage(payload, shortChannelIdTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing ChannelReadyMessage", e);
        }
    }
}