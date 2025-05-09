namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Payloads;
using Tlvs;
using ValueObjects;

/// <summary>
/// Represents a closing_signed message.
/// </summary>
/// <remarks>
/// The closing_signed message is after shutdown is complete and there are no pending HTLCs.
/// The message type is 39.
/// </remarks>
public sealed class ClosingSignedMessage : BaseMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new ClosingSignedPayload Payload { get => (ClosingSignedPayload)base.Payload; }

    public FeeRangeTlv FeeRangeTlv { get; }

    public ClosingSignedMessage(ClosingSignedPayload payload, FeeRangeTlv feeRangeTlv) : base(MessageTypes.CLOSING_SIGNED, payload)
    {
        FeeRangeTlv = feeRangeTlv;
        Extension = new TlvStream();
        Extension.Add(feeRangeTlv);
    }
}