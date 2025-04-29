using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;
using TLVs;
using Types;

/// <summary>
/// Represents an open_channel message.
/// </summary>
/// <remarks>
/// The accept_channel message is sent to the initiator in order to accept the channel opening.
/// The message type is 33.
/// </remarks>
public sealed class AcceptChannel1Message : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new AcceptChannel1Payload Payload { get => (AcceptChannel1Payload)base.Payload; }

    /// <summary>
    /// Optional UpfrontShutdownScriptTlv
    /// </summary>
    public UpfrontShutdownScriptTlv? UpfrontShutdownScriptTlv { get; }

    /// <summary>
    /// Optional ChannelTypeTlv
    /// </summary>
    public ChannelTypeTlv? ChannelTypeTlv { get; }

    public AcceptChannel1Message(AcceptChannel1Payload payload,
                                 UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null,
                                 ChannelTypeTlv? channelTypeTlv = null)
        : base(MessageTypes.ACCEPT_CHANNEL_2, payload)
    {
        UpfrontShutdownScriptTlv = upfrontShutdownScriptTlv;
        ChannelTypeTlv = channelTypeTlv;

        if (UpfrontShutdownScriptTlv is not null || ChannelTypeTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(UpfrontShutdownScriptTlv, ChannelTypeTlv);
        }
    }

    /// <summary>
    /// Deserialize a OpenChannel1Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized AcceptChannel1Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel1Message</exception>
    public static async Task<AcceptChannel1Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await AcceptChannel1Payload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new AcceptChannel1Message(payload);
            }

            var upfrontShutdownScriptTlv = extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var tlv)
                ? UpfrontShutdownScriptTlv.FromTlv(tlv!)
                : null;

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.CHANNEL_TYPE, out tlv)
                ? ChannelTypeTlv.FromTlv(tlv!)
                : null;

            return new AcceptChannel1Message(payload, upfrontShutdownScriptTlv, channelTypeTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing AcceptChannel1Message", e);
        }
    }
}