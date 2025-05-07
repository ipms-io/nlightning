namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Models;
using Payloads;
using Tlv;

/// <summary>
/// Represents a channel_reestablish message.
/// </summary>
/// <remarks>
/// The channel_reestablish message is sent when a connection is lost.
/// The message type is 136.
/// </remarks>
public sealed class ChannelReestablishMessage : BaseChannelMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ChannelReestablishPayload Payload { get => (ChannelReestablishPayload)base.Payload; }

    public NextFundingTlv? NextFundingTlv { get; }

    public ChannelReestablishMessage(ChannelReestablishPayload payload, NextFundingTlv? nextFundingTlv = null)
        : base(MessageTypes.ChannelReestablish, payload)
    {
        NextFundingTlv = nextFundingTlv;

        if (NextFundingTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(NextFundingTlv);
        }
    }
}