using NBitcoin;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Managers;
using Exceptions;
using Interfaces;

/// <summary>
/// Represents the payload for the open_channel2 message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the OpenChannel2Payload class.
/// This class depends on the initialized singleton class <see cref="Common.Managers.ConfigManager"/>.
/// </remarks>
public class OpenChannel2Payload : IMessagePayload
{
    /// <summary>
    /// The chain_hash value denotes the exact blockchain that the opened channel will reside within.
    /// </summary>
    public ChainHash ChainHash { get; }

    /// <summary>
    /// The temporary_channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId TemporaryChannelId { get; set; }

    /// <summary>
    /// funding_feerate_perkw indicates the fee rate that the opening node will pay for the funding transaction in
    /// satoshi per 1000-weight, as described in BOLT-3, Appendix F
    /// </summary>
    public uint FundingFeeRatePerKw { get; set; }

    /// <summary>
    /// commitment_feerate_perkw indicates the fee rate that will be paid for the commitment transaction in
    /// satoshi per 1000-weight, as described in BOLT-3, Appendix F
    /// </summary>
    public uint CommitmentFeeRatePerKw { get; set; }

    /// <summary>
    /// funding_satoshis is the amount the sender is putting into the channel.
    /// </summary>
    public ulong FundingSatoshis { get; set; }

    /// <summary>
    /// dust_limit_satoshis is the threshold below which outputs should not be generated for this node's commitment or
    /// HTLC transactions
    /// </summary>
    public ulong DustLimitSatoshis { get; }

    /// <summary>
    /// max_htlc_value_in_flight_msat is a cap on total value of outstanding HTLCs offered by the remote node, which
    /// allows the local node to limit its exposure to HTLCs
    /// </summary>
    public ulong MaxHtlcValueInFlightMsat { get; }

    /// <summary>
    /// htlc_minimum_msat indicates the smallest value HTLC this node will accept.
    /// </summary>
    public ulong HtlcMinimumMsat { get; }

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
    /// locktime is the locktime for the funding transaction.
    /// </summary>
    public uint Locktime { get; }

    /// <summary>
    /// funding_pubkey is the public key in the 2-of-2 multisig script of the funding transaction output.
    /// </summary>
    public PubKey FundingPubKey { get; set; }

    /// <summary>
    /// revocation_basepoint is used to regenerate the scripts required for the penalty transaction
    /// </summary>
    public PubKey RevocationBasepoint { get; set; }

    /// <summary>
    /// payment_basepoint is used to produce payment signatures for the protocol
    /// </summary>
    public PubKey PaymentBasepoint { get; set; }

    /// <summary>
    /// delayed_payment_basepoint is used to regenerate the scripts required for the penalty transaction
    /// </summary>
    public PubKey DelayedPaymentBasepoint { get; set; }

    /// <summary>
    /// htlc_basepoint is used to produce HTLC signatures for the protocol
    /// </summary>
    public PubKey HtlcBasepoint { get; set; }

    /// <summary>
    /// first_per_commitment_point is the per-commitment point used for the first commitment transaction
    /// </summary>
    public PubKey FirstPerCommitmentPoint { get; set; }

    /// <summary>
    /// second_per_commitment_point is the per-commitment point used for the first commitment transaction
    /// </summary>
    public PubKey SecondPerCommitmentPoint { get; set; }

    /// <summary>
    /// Only the least-significant bit of channel_flags is currently defined: announce_channel. This indicates whether
    /// the initiator of the funding flow wishes to advertise this channel publicly to the network
    /// </summary>
    public ChannelFlags ChannelFlags { get; set; }

    public OpenChannel2Payload(ChannelId temporaryChannelId, uint fundingFeeRatePerKw, uint commitmentFeeRatePerKw, ulong fundingSatoshis, PubKey fundingPubKey, PubKey revocationBasepoint, PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint, PubKey firstPerCommitmentPoint, PubKey secondPerCommitmentPoint, ChannelFlags channelFlags)
    {
        ChainHash = ConfigManager.Instance.Network.ChainHash;
        DustLimitSatoshis = ConfigManager.Instance.DustLimitAmountSats;
        MaxHtlcValueInFlightMsat = ConfigManager.Instance.MaxHtlcValueInFlightMsat;
        HtlcMinimumMsat = ConfigManager.Instance.HtlcMinimumMsat;
        ToSelfDelay = ConfigManager.Instance.ToSelfDelay;
        MaxAcceptedHtlcs = ConfigManager.Instance.MaxAcceptedHtlcs;
        Locktime = ConfigManager.Instance.Locktime;
        TemporaryChannelId = temporaryChannelId;
        FundingFeeRatePerKw = fundingFeeRatePerKw;
        CommitmentFeeRatePerKw = commitmentFeeRatePerKw;
        FundingSatoshis = fundingSatoshis;
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
        FirstPerCommitmentPoint = firstPerCommitmentPoint;
        SecondPerCommitmentPoint = secondPerCommitmentPoint;
        ChannelFlags = channelFlags;
    }

    private OpenChannel2Payload(ChainHash chainHash, ChannelId temporaryChannelId, uint fundingFeeRatePerKw, uint commitmentFeeRatePerKw, ulong fundingSatoshis, ulong dustLimitSatoshis, ulong maxHtlcValueInFlightMsat, ulong htlcMinimumMsat, ushort toSelfDelay, ushort maxAcceptedHtlcs, uint locktime, PubKey fundingPubKey, PubKey revocationBasepoint, PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint, PubKey firstPerCommitmentPoint, PubKey secondPerCommitmentPoint, ChannelFlags channelFlags)
    {
        ChainHash = chainHash;
        TemporaryChannelId = temporaryChannelId;
        FundingFeeRatePerKw = fundingFeeRatePerKw;
        CommitmentFeeRatePerKw = commitmentFeeRatePerKw;
        FundingSatoshis = fundingSatoshis;
        DustLimitSatoshis = dustLimitSatoshis;
        MaxHtlcValueInFlightMsat = maxHtlcValueInFlightMsat;
        HtlcMinimumMsat = htlcMinimumMsat;
        ToSelfDelay = toSelfDelay;
        MaxAcceptedHtlcs = maxAcceptedHtlcs;
        Locktime = locktime;
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
        FirstPerCommitmentPoint = firstPerCommitmentPoint;
        SecondPerCommitmentPoint = secondPerCommitmentPoint;
        ChannelFlags = channelFlags;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChainHash.SerializeAsync(stream);
        await TemporaryChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FundingFeeRatePerKw));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(CommitmentFeeRatePerKw));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FundingSatoshis));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(DustLimitSatoshis));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MaxHtlcValueInFlightMsat));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(HtlcMinimumMsat));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MaxAcceptedHtlcs));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(Locktime));
        await stream.WriteAsync(FundingPubKey.ToBytes());
        await stream.WriteAsync(RevocationBasepoint.ToBytes());
        await stream.WriteAsync(PaymentBasepoint.ToBytes());
        await stream.WriteAsync(DelayedPaymentBasepoint.ToBytes());
        await stream.WriteAsync(HtlcBasepoint.ToBytes());
        await stream.WriteAsync(FirstPerCommitmentPoint.ToBytes());
        await stream.WriteAsync(SecondPerCommitmentPoint.ToBytes());
        await ChannelFlags.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<OpenChannel2Payload> DeserializeAsync(Stream stream)
    {
        try
        {
            var chainHash = await ChainHash.DeserializeAsync(stream);
            var temporaryChannelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[sizeof(uint)];
            await stream.ReadExactlyAsync(bytes);
            var fundingFeeRatePerKw = EndianBitConverter.ToUInt32BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var commitmentFeeRatePerKw = EndianBitConverter.ToUInt32BigEndian(bytes);

            bytes = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(bytes);
            var fundingSatoshis = EndianBitConverter.ToUInt64BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var dustLimitSatoshis = EndianBitConverter.ToUInt64BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var maxHtlcValueInFlightMsat = EndianBitConverter.ToUInt64BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var htlcMinimumMsat = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(bytes);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(bytes);

            bytes = new byte[sizeof(uint)];
            await stream.ReadExactlyAsync(bytes);
            var locktime = EndianBitConverter.ToUInt32BigEndian(bytes);

            bytes = new byte[33];
            await stream.ReadExactlyAsync(bytes);
            var fundingPubKey = new PubKey(bytes);

            await stream.ReadExactlyAsync(bytes);
            var revocationBasepoint = new PubKey(bytes);

            await stream.ReadExactlyAsync(bytes);
            var paymentBasepoint = new PubKey(bytes);

            await stream.ReadExactlyAsync(bytes);
            var delayedPaymentBasepoint = new PubKey(bytes);

            await stream.ReadExactlyAsync(bytes);
            var htlcBasepoint = new PubKey(bytes);

            await stream.ReadExactlyAsync(bytes);
            var firstPerCommitmentPoint = new PubKey(bytes);

            await stream.ReadExactlyAsync(bytes);
            var secondPerCommitmentPoint = new PubKey(bytes);

            var channelFlags = await ChannelFlags.DeserializeAsync(stream);

            return new OpenChannel2Payload(chainHash, temporaryChannelId, fundingFeeRatePerKw, commitmentFeeRatePerKw, fundingSatoshis, dustLimitSatoshis, maxHtlcValueInFlightMsat, htlcMinimumMsat, toSelfDelay, maxAcceptedHtlcs, locktime, fundingPubKey, revocationBasepoint, paymentBasepoint, delayedPaymentBasepoint, htlcBasepoint, firstPerCommitmentPoint, secondPerCommitmentPoint, channelFlags);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing OpenChannel2Payload", e);
        }
    }
}