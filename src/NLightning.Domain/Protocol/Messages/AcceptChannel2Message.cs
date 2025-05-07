namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Models;
using Payloads;
using Tlv;

/// <summary>
/// Represents an open_channel2 message.
/// </summary>
/// <remarks>
/// The accept_channel2 message is sent to the initiator to accept the channel opening.
/// The message type is 65.
/// </remarks>
public sealed class AcceptChannel2Message : BaseChannelMessage
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
        : base(MessageTypes.AcceptChannel2, payload)
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