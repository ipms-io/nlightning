namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Models;
using Payloads;
using Tlv;

/// <summary>
/// Represents an open_channel message.
/// </summary>
/// <remarks>
/// The accept_channel message is sent to the initiator in order to accept the channel opening.
/// The message type is 33.
/// </remarks>
public sealed class AcceptChannel1Message : BaseChannelMessage
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
        : base(MessageTypes.AcceptChannel2, payload)
    {
        UpfrontShutdownScriptTlv = upfrontShutdownScriptTlv;
        ChannelTypeTlv = channelTypeTlv;

        if (UpfrontShutdownScriptTlv is not null || ChannelTypeTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(UpfrontShutdownScriptTlv, ChannelTypeTlv);
        }
    }
}