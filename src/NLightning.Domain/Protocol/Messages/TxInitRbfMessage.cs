namespace NLightning.Domain.Protocol.Messages;

using Constants;
using Models;
using Payloads;
using Tlv;

/// <summary>
/// Represents a tx_init_rbf message.
/// </summary>
/// <remarks>
/// The tx_init_rbf message initiates a replacement of the transaction after it's been completed.
/// The message type is 72.
/// </remarks>
public sealed class TxInitRbfMessage : BaseChannelMessage
{
    /// <summary>
    /// The payload of the message.
    /// </summary>
    public new TxInitRbfPayload Payload { get => (TxInitRbfPayload)base.Payload; }

    public FundingOutputContributionTlv? FundingOutputContributionTlv { get; }
    public RequireConfirmedInputsTlv? RequireConfirmedInputsTlv { get; }

    public TxInitRbfMessage(TxInitRbfPayload payload, FundingOutputContributionTlv? fundingOutputContributionTlv = null, RequireConfirmedInputsTlv? requireConfirmedInputsTlv = null)
        : base(MessageTypes.TxInitRbf, payload)
    {
        FundingOutputContributionTlv = fundingOutputContributionTlv;
        RequireConfirmedInputsTlv = requireConfirmedInputsTlv;

        if (FundingOutputContributionTlv is not null || RequireConfirmedInputsTlv is not null)
        {
            Extension = new TlvStream();
            Extension.Add(FundingOutputContributionTlv, RequireConfirmedInputsTlv);
        }
    }
}