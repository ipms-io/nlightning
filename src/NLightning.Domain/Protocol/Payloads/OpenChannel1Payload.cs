using NBitcoin;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Money;
using ValueObjects;

/// <summary>
/// Represents the payload for the open_channel message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the OpenChannel1Payload class.
/// </remarks>
public class OpenChannel1Payload : IChannelMessagePayload
{
    /// <summary>
    /// The chain_hash value denotes the exact blockchain that the opened channel will reside within.
    /// </summary>
    public ChainHash ChainHash { get; }

    /// <summary>
    /// The temporary_channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// funding_satoshis is the amount the sender is putting into the channel.
    /// </summary>
    /// <remarks>Amount is used in Satoshis</remarks>
    public LightningMoney FundingAmount { get; }

    /// <summary>
    /// push_msat is the amount the sender is pushing to the receiver of the channel.
    /// </summary>
    /// <remarks>Amount is used in Millisatoshis</remarks>
    public LightningMoney PushAmount { get; }

    /// <summary>
    /// dust_limit_satoshis is the threshold below which outputs should not be generated for this node's commitment or
    /// HTLC transactions
    /// </summary>
    public LightningMoney DustLimitAmount { get; }

    /// <summary>
    /// max_htlc_value_in_flight_msat is a cap on total value of outstanding HTLCs offered by the remote node, which
    /// allows the local node to limit its exposure to HTLCs
    /// </summary>
    public LightningMoney MaxHtlcValueInFlight { get; }

    /// <summary>
    /// channel_reserve_satoshis is the amount that must remain in the channel after a commitment transaction
    /// </summary>
    public LightningMoney ChannelReserveAmount { get; }

    /// <summary>
    /// htlc_minimum_msat indicates the smallest value HTLC this node will accept.
    /// </summary>
    public LightningMoney HtlcMinimumAmount { get; }

    /// <summary>
    /// feerate_per_kw is the fee rate that will be paid for the commitment transaction in
    /// satoshi per 1000-weight
    /// </summary>
    public LightningMoney FeeRatePerKw { get; }

    /// <summary>
    /// to_self_delay is how long (in blocks) the other node will have to wait in case of breakdown before redeeming
    /// its own funds.
    /// </summary>
    public ushort ToSelfDelay { get; }

    /// <summary>
    /// max_accepted_htlcs limits the number of outstanding HTLCs the remote node can offer.
    /// </summary>
    public ushort MaxAcceptedHtlcs { get; }

    /// <summary>
    /// funding_pubkey is the public key in the 2-of-2 multisig script of the funding transaction output.
    /// </summary>
    public PubKey FundingPubKey { get; }

    /// <summary>
    /// revocation_basepoint is used to regenerate the scripts required for the penalty transaction
    /// </summary>
    public PubKey RevocationBasepoint { get; }

    /// <summary>
    /// payment_basepoint is used to produce payment signatures for the protocol
    /// </summary>
    public PubKey PaymentBasepoint { get; }

    /// <summary>
    /// delayed_payment_basepoint is used to regenerate the scripts required for the penalty transaction
    /// </summary>
    public PubKey DelayedPaymentBasepoint { get; }

    /// <summary>
    /// htlc_basepoint is used to produce HTLC signatures for the protocol
    /// </summary>
    public PubKey HtlcBasepoint { get; }

    /// <summary>
    /// first_per_commitment_point is the per-commitment point used for the first commitment transaction
    /// </summary>
    public PubKey FirstPerCommitmentPoint { get; }

    /// <summary>
    /// Only the least-significant bit of channel_flags is currently defined: announce_channel. This indicates whether
    /// the initiator of the funding flow wishes to advertise this channel publicly to the network
    /// </summary>
    public ChannelFlags ChannelFlags { get; }

    public OpenChannel1Payload(ChainHash chainHash, ChannelId channelId, LightningMoney fundingAmount,
                               LightningMoney pushAmount, LightningMoney dustLimitAmount,
                               LightningMoney maxHtlcValueInFlight, LightningMoney channelReserveAmount,
                               LightningMoney htlcMinimumAmount, LightningMoney feeRatePerKw, ushort toSelfDelay,
                               ushort maxAcceptedHtlcs, PubKey fundingPubKey, PubKey revocationBasepoint,
                               PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint,
                               PubKey firstPerCommitmentPoint, ChannelFlags channelFlags)
    {
        ChainHash = chainHash;
        ChannelId = channelId;
        FundingAmount = fundingAmount;
        PushAmount = pushAmount;
        DustLimitAmount = dustLimitAmount;
        MaxHtlcValueInFlight = maxHtlcValueInFlight;
        ChannelReserveAmount = channelReserveAmount;
        HtlcMinimumAmount = htlcMinimumAmount;
        FeeRatePerKw = feeRatePerKw;
        ToSelfDelay = toSelfDelay;
        MaxAcceptedHtlcs = maxAcceptedHtlcs;
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
        FirstPerCommitmentPoint = firstPerCommitmentPoint;
        ChannelFlags = channelFlags;
    }
}