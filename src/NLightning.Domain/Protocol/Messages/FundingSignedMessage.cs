namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;

/// <summary>
/// Represents a funding_signed message.
/// </summary>
/// <remarks>
/// The funding_signed message is sent by the funder to the fundee after the funding transaction has been created.
/// The message type is 35.
/// </remarks>
public sealed class FundingSignedMessage : BaseChannelMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new FundingSignedPayload Payload { get => (FundingSignedPayload)base.Payload; }

    public FundingSignedMessage(FundingSignedPayload payload) : base(MessageTypes.AcceptChannel2, payload)
    { }
}