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
/// The open_channel2 message is sent to another peer in order to start the channel negotiation.
/// The message type is 64.
/// </remarks>
public sealed class OpenChannel2Message : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new OpenChannel2Payload Payload { get => (OpenChannel2Payload)base.Payload; }

    public UpfrontShutdownScriptTlv? UpfrontShutdownScript { get; }
    public ChannelTypeTlv? ChannelType { get; }
    public RequireConfirmedInputsTlv? RequireConfirmedInputs { get; }

    public OpenChannel2Message(OpenChannel2Payload payload, UpfrontShutdownScriptTlv? upfrontShutdownScript = null, ChannelTypeTlv? channelType = null, RequireConfirmedInputsTlv? requireConfirmedInputs = null)
        : base(MessageTypes.OPEN_CHANNEL_2, payload)
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
    /// <returns>The deserialized OpenChannel2Message.</returns>
    /// <exception cref="MessageSerializationException">Error deserializing OpenChannel2Message</exception>
    public static async Task<OpenChannel2Message> DeserializeAsync(Stream stream)
    {
        try
        {
            // Deserialize payload
            var payload = await OpenChannel2Payload.DeserializeAsync(stream);

            // Deserialize extension
            var extension = await TlvStream.DeserializeAsync(stream);
            if (extension is null)
            {
                return new OpenChannel2Message(payload);
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

            return new OpenChannel2Message(payload, upfrontShutdownScript, channelType, requireConfirmedInputs);
        }
        catch (SerializationException e)
        {
            throw new MessageSerializationException("Error deserializing OpenChannel2Message", e);
        }
    }
}