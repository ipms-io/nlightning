namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Models;
using Payloads;
using Tlv;

/// <summary>
/// Represents an open_channel2 message.
/// </summary>
/// <remarks>
/// The open_channel2 message is sent to another peer in order to start the channel negotiation.
/// The message type is 64.
/// </remarks>
public sealed class OpenChannel2Message : BaseChannelMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new OpenChannel2Payload Payload { get => (OpenChannel2Payload)base.Payload; }

    public UpfrontShutdownScriptTlv? UpfrontShutdownScriptTlv { get; }
    public ChannelTypeTlv? ChannelTypeTlv { get; }
    public RequireConfirmedInputsTlv? RequireConfirmedInputsTlv { get; }

    public OpenChannel2Message(OpenChannel2Payload payload, UpfrontShutdownScriptTlv? upfrontShutdownScriptTlv = null, ChannelTypeTlv? channelTypeTlv = null, RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null)
        : base(MessageTypes.OpenChannel2, payload)
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
}