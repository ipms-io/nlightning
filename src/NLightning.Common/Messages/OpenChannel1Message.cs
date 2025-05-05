using System.Runtime.Serialization;

namespace NLightning.Common.Messages;

using Constants;
using Exceptions;
using Payloads;
using TLVs;
using Types;

/// <summary>
/// Represents an open_channel2 message.
/// </summary>
/// <remarks>
/// The open_channel message is sent to another peer in order to start the channel negotiation.
/// The message type is 32.
/// </remarks>
public sealed class OpenChannel1Message : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new OpenChannel1Payload Payload { get => (OpenChannel1Payload)base.Payload; }

    public UpfrontShutdownScriptTlv? UpfrontShutdownScriptTlv { get; }
    public ChannelTypeTlv? ChannelTypeTlv { get; }

    public OpenChannel1Message(OpenChannel1Payload payload, UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null,
                               ChannelTypeTlv? channelTypeTlv = null)
        : base(MessageTypes.OPEN_CHANNEL_2, payload)
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
    /// <returns>The deserialized OpenChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public static async Task<OpenChannel1Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await OpenChannel1Payload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new OpenChannel1Message(payload);
            }

            var upfrontShutdownScriptTlv = extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var tlv)
                ? UpfrontShutdownScriptTlv.FromTlv(tlv!)
                : null;

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.CHANNEL_TYPE, out tlv)
                ? ChannelTypeTlv.FromTlv(tlv!)
                : null;

            return new OpenChannel1Message(payload, upfrontShutdownScriptTlv, channelTypeTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing OpenChannel1Message", e);
        }
    }
}