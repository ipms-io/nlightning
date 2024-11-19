using System.Runtime.Serialization;

namespace NLightning.Bolts.BOLT2.Messages;

using Base;
using Bolts.Constants;
using Common.Constants;
using Common.TLVs;
using Exceptions;
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
    public UpfrontShutdownScriptTlv? UpfrontShutdownScript { get; }

    /// <summary>
    /// Optional ChannelTypeTlv
    /// </summary>
    public ChannelTypeTlv? ChannelType { get; }

    /// <summary>
    /// Optional RequireConfirmedInputsTlv
    /// </summary>
    public RequireConfirmedInputsTlv? RequireConfirmedInputs { get; }

    public AcceptChannel2Message(AcceptChannel2Payload payload, UpfrontShutdownScriptTlv? upfrontShutdownScript = null, ChannelTypeTlv? channelType = null, RequireConfirmedInputsTlv? requireConfirmedInputs = null)
        : base(MessageTypes.ACCEPT_CHANNEL_2, payload)
    {
        UpfrontShutdownScript = upfrontShutdownScript;
        ChannelType = channelType;
        RequireConfirmedInputs = requireConfirmedInputs;

        if (UpfrontShutdownScript is not null || ChannelType is not null || RequireConfirmedInputs is not null)
        {
            Extension = new TlvStream();
            Extension.Add(UpfrontShutdownScript, ChannelType, RequireConfirmedInputs);
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

            var upfrontShutdownScript = extension.TryGetTlv(TlvConstants.UPFRONT_SHUTDOWN_SCRIPT, out var upfrontShutdownScriptTlv)
                ? UpfrontShutdownScriptTlv.FromTlv(upfrontShutdownScriptTlv!)
                : null;

            var channelType = extension.TryGetTlv(TlvConstants.CHANNEL_TYPE, out var channelTypeTlv)
                ? ChannelTypeTlv.FromTlv(channelTypeTlv!)
                : null;

            var requireConfirmedInputs = extension.TryGetTlv(TlvConstants.REQUIRE_CONFIRMED_INPUTS, out var requireConfirmedInputsTlv)
                ? RequireConfirmedInputsTlv.FromTlv(requireConfirmedInputsTlv!)
                : null;

            return new AcceptChannel2Message(payload, upfrontShutdownScript, channelType, requireConfirmedInputs);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing AcceptChannel2Message", e);
        }
    }
}