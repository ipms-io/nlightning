using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Common.Constants;
using Common.TLVs;
using Payloads;

/// <summary>
/// Represents an open_channel2 message.
/// </summary>
/// <remarks>
/// The accept_channel2 message is sent to the initiator in order to accept the channel opening.
/// The message type is 65.
/// </remarks>
public sealed class AcceptChannel2Message : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new AcceptChannel2Payload Payload { get => (AcceptChannel2Payload)base.Payload; }

    /// <summary>
    /// Optional UpfrontShutdownScriptTlv
    /// </summary>
    public UpfrontShutdownScriptTlv? UpfrontShutdownScriptTlv { get; }

    /// <summary>
    /// Optional ChannelTypeTlv
    /// </summary>
    public ChannelTypeTlv? ChannelTypeTlv { get; }

    /// <summary>
    /// Optional RequireConfirmedInputsTlv
    /// </summary>
    public RequireConfirmedInputsTlv? RequireConfirmedInputsTlv { get; }

    public AcceptChannel2Message(AcceptChannel2Payload payload, UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null, ChannelTypeTlv? channelTypeTlv = null, RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null)
        : base(MessageTypes.ACCEPT_CHANNEL_2, payload)
    {
        UpfrontShutdownScriptTlv = upfrontShutdownScriptTlv;
        ChannelTypeTlv = channelTypeTlv;
        RequireConfirmedInputsTlv = requireConfirmedInputsTlv;

        if (UpfrontShutdownScriptTlv is not null || ChannelTypeTlv is not null || RequireConfirmedInputsTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(UpfrontShutdownScriptTlv, ChannelTypeTlv, RequireConfirmedInputsTlv);
        }
    }

    /// <summary>
    /// Deserialize a OpenChannel2Message from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized AcceptChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public static async Task<AcceptChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await AcceptChannel2Payload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new AcceptChannel2Message(payload);
            }

            var upfrontShutdownScriptTlv = extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var tlv)
                ? UpfrontShutdownScriptTlv.FromTlv(tlv!)
                : null;

            var channelTypeTlv = extension.TryGetTlv(TlvConstants.CHANNEL_TYPE, out tlv)
                ? ChannelTypeTlv.FromTlv(tlv!)
                : null;

            var requireConfirmedInputsTlv = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out tlv)
                ? RequireConfirmedInputsTlv.FromTlv(tlv!)
                : null;

            return new AcceptChannel2Message(payload, upfrontShutdownScriptTlv, channelTypeTlv, requireConfirmedInputsTlv);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing AcceptChannel2Message", e);
        }
    }
}