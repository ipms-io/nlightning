using System.Runtime.Serialization;
using NBitcoin;

namespace NLightning.Bolts.BOLT2.Payloads;

using Common.BitUtils;
using Common.Managers;
using Interfaces;

/// <summary>
/// Represents the payload for the accept_channel2 message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the AcceptChannel2Payload class.
/// This class depends on the initialized singleton class <see cref="Common.Managers.ConfigManager"/>.
/// </remarks>
public class AcceptChannel2Payload : IMessagePayload
{
    /// <summary>
    /// The temporary_channel_id is used to identify this channel on a per-peer basis until the funding transaction
    /// is established, at which point it is replaced by the channel_id, which is derived from the funding transaction.
    /// </summary>
    public ChannelId TemporaryChannelId { get; set; }

    /// <summary>
    /// funding_satoshis is the amount the acceptor is putting into the channel.
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
    /// minimum_depth is the number of blocks we consider reasonable to avoid double-spending of the funding transaction.
    /// In case channel_type includes option_zeroconf this MUST be 0
    /// </summary>
    public uint MinimumDepth { get; set; }

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
    /// 
    /// </summary>
    /// <see cref="Common.TLVs.UpfrontShutdownScriptTlv"/>
    public TlvStream? AcceptTlvs { get; set; }

    public AcceptChannel2Payload(ChannelId temporaryChannelId, ulong fundingSatoshis, PubKey fundingPubKey, PubKey revocationBasepoint, PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint, PubKey firstPerCommitmentPoint, TlvStream? acceptTlvs = null)
    {
        DustLimitSatoshis = ConfigManager.Instance.DustLimitSatoshis;
        MaxHtlcValueInFlightMsat = ConfigManager.Instance.MaxHtlcValueInFlightMsat;
        HtlcMinimumMsat = ConfigManager.Instance.HtlcMinimumMsat;
        MinimumDepth = ConfigManager.Instance.MinimumDepth;
        ToSelfDelay = ConfigManager.Instance.ToSelfDelay;
        MaxAcceptedHtlcs = ConfigManager.Instance.MaxAcceptedHtlcs;
        TemporaryChannelId = temporaryChannelId;
        FundingSatoshis = fundingSatoshis;
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
        FirstPerCommitmentPoint = firstPerCommitmentPoint;
        AcceptTlvs = acceptTlvs;
    }

    private AcceptChannel2Payload(ChannelId temporaryChannelId, ulong fundingSatoshis, ulong dustLimitSatoshis, ulong maxHtlcValueInFlightMsat, ulong htlcMinimumMsat, uint minimumDepth, ushort toSelfDelay, ushort maxAcceptedHtlcs, PubKey fundingPubKey, PubKey revocationBasepoint, PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint, PubKey firstPerCommitmentPoint, TlvStream? acceptTlvs = null)
    {
        TemporaryChannelId = temporaryChannelId;
        FundingSatoshis = fundingSatoshis;
        DustLimitSatoshis = dustLimitSatoshis;
        MaxHtlcValueInFlightMsat = maxHtlcValueInFlightMsat;
        HtlcMinimumMsat = htlcMinimumMsat;
        MinimumDepth = minimumDepth;
        ToSelfDelay = toSelfDelay;
        MaxAcceptedHtlcs = maxAcceptedHtlcs;
        FundingPubKey = fundingPubKey;
        RevocationBasepoint = revocationBasepoint;
        PaymentBasepoint = paymentBasepoint;
        DelayedPaymentBasepoint = delayedPaymentBasepoint;
        HtlcBasepoint = htlcBasepoint;
        FirstPerCommitmentPoint = firstPerCommitmentPoint;
        AcceptTlvs = acceptTlvs;
    }

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await TemporaryChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FundingSatoshis));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(DustLimitSatoshis));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MaxHtlcValueInFlightMsat));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(HtlcMinimumMsat));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MinimumDepth));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MaxAcceptedHtlcs));
        await stream.WriteAsync(FundingPubKey.ToBytes());
        await stream.WriteAsync(RevocationBasepoint.ToBytes());
        await stream.WriteAsync(PaymentBasepoint.ToBytes());
        await stream.WriteAsync(DelayedPaymentBasepoint.ToBytes());
        await stream.WriteAsync(HtlcBasepoint.ToBytes());
        await stream.WriteAsync(FirstPerCommitmentPoint.ToBytes());
        if (AcceptTlvs is not null)
        {
            await AcceptTlvs.SerializeAsync(stream);
        }
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="SerializationException">Error deserializing Payload</exception>
    public static async Task<AcceptChannel2Payload> DeserializeAsync(Stream stream)
    {
        try
        {
            var temporaryChannelId = await ChannelId.DeserializeAsync(stream);

            var bytes = new byte[sizeof(ulong)];
            await stream.ReadExactlyAsync(bytes);
            var fundingSatoshis = EndianBitConverter.ToUInt64BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var dustLimitSatoshis = EndianBitConverter.ToUInt64BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var maxHtlcValueInFlightMsat = EndianBitConverter.ToUInt64BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var htlcMinimumMsat = EndianBitConverter.ToUInt64BigEndian(bytes);

            bytes = new byte[sizeof(uint)];
            await stream.ReadExactlyAsync(bytes);
            var minimumDepth = EndianBitConverter.ToUInt32BigEndian(bytes);

            bytes = new byte[sizeof(ushort)];
            await stream.ReadExactlyAsync(bytes);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(bytes);

            await stream.ReadExactlyAsync(bytes);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(bytes);

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

            var openingTlvs = await TlvStream.DeserializeAsync(stream);

            return new AcceptChannel2Payload(temporaryChannelId, fundingSatoshis, dustLimitSatoshis, maxHtlcValueInFlightMsat, htlcMinimumMsat, minimumDepth, toSelfDelay, maxAcceptedHtlcs, fundingPubKey, revocationBasepoint, paymentBasepoint, delayedPaymentBasepoint, htlcBasepoint, firstPerCommitmentPoint, openingTlvs);
        }
        catch (Exception e)
        {
            throw new SerializationException("Error deserializing AcceptChannel2Payload", e);
        }
    }
}