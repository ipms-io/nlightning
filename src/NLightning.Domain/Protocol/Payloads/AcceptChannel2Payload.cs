using NBitcoin;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Money;
using ValueObjects;

/// <summary>
/// Represents the payload for the accept_channel2 message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AcceptChannel2Payload class.
/// </remarks>
public class AcceptChannel2Payload(PubKey delayedPaymentBasepoint, LightningMoney dustLimitAmount,
                                   PubKey firstPerCommitmentPoint, LightningMoney fundingAmount, PubKey fundingPubKey,
                                   PubKey htlcBasepoint, LightningMoney htlcMinimumAmount, ushort maxAcceptedHtlcs,
                                   LightningMoney maxHtlcValueInFlight, uint minimumDepth, PubKey paymentBasepoint,
                                   PubKey revocationBasepoint, ChannelId channelId, ushort toSelfDelay)
    : IMessagePayload
{
    /// <summary>
    /// The temporary_channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// funding_satoshis is the amount the acceptor is putting into the channel.
    /// </summary>
    public LightningMoney FundingAmount { get; } = fundingAmount;

    /// <summary>
    /// dust_limit_satoshis is the threshold below which outputs should not be generated for this node's commitment or
    /// HTLC transactions
    /// </summary>
    public LightningMoney DustLimitAmount { get; } = dustLimitAmount;

    /// <summary>
    /// max_htlc_value_in_flight_msat is a cap on total value of outstanding HTLCs offered by the remote node, which
    /// allows the local node to limit its exposure to HTLCs
    /// </summary>
    public LightningMoney MaxHtlcValueInFlightAmount { get; } = maxHtlcValueInFlight;

    /// <summary>
    /// htlc_minimum_msat indicates the smallest value HTLC this node will accept.
    /// </summary>
    public LightningMoney HtlcMinimumAmount { get; } = htlcMinimumAmount;

    /// <summary>
    /// minimum_depth is the number of blocks we consider reasonable to avoid double-spending of the funding transaction.
    /// In case channel_type includes option_zeroconf this MUST be 0
    /// </summary>
    public uint MinimumDepth { get; } = minimumDepth;

    /// <summary>
    /// to_self_delay is how long (in blocks) the other node will have to wait in case of breakdown before redeeming
    /// its own funds.
    /// </summary>
    public ushort ToSelfDelay { get; } = toSelfDelay;

    /// <summary>
    /// max_accepted_htlcs limits the number of outstanding HTLCs the remote node can offer.
    /// </summary>
    public ushort MaxAcceptedHtlcs { get; } = maxAcceptedHtlcs;

    /// <summary>
    /// funding_pubkey is the public key in the 2-of-2 multisig script of the funding transaction output.
    /// </summary>
    public PubKey FundingPubKey { get; } = fundingPubKey;

    /// <summary>
    /// revocation_basepoint is used to regenerate the scripts required for the penalty transaction
    /// </summary>
    public PubKey RevocationBasepoint { get; } = revocationBasepoint;

    /// <summary>
    /// payment_basepoint is used to produce payment signatures for the protocol
    /// </summary>
    public PubKey PaymentBasepoint { get; } = paymentBasepoint;

    /// <summary>
    /// delayed_payment_basepoint is used to regenerate the scripts required for the penalty transaction
    /// </summary>
    public PubKey DelayedPaymentBasepoint { get; } = delayedPaymentBasepoint;

    /// <summary>
    /// htlc_basepoint is used to produce HTLC signatures for the protocol
    /// </summary>
    public PubKey HtlcBasepoint { get; } = htlcBasepoint;

    /// <summary>
    /// first_per_commitment_point is the per-commitment point used for the first commitment transaction
    /// </summary>
    public PubKey FirstPerCommitmentPoint { get; } = firstPerCommitmentPoint;
}