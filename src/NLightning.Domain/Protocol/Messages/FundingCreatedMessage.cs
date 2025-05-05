namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a funding_created message.
/// </summary>
/// <remarks>
/// The funding_created message is sent by the funder to the fundee after the funding transaction has been created.
/// The message type is 34.
/// </remarks>
public sealed class FundingCreatedMessage : BaseChannelMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new FundingCreatedPayload Payload { get => (FundingCreatedPayload)base.Payload; }

    public FundingCreatedMessage(FundingCreatedPayload payload) : base(MessageTypes.ACCEPT_CHANNEL_2, payload)
    { }
}