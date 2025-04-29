using System.Buffers;
using NBitcoin;

namespace NLightning.Common.Messages.Payloads;

using BitUtils;
using Exceptions;
using Interfaces;
using Types;

/// <summary>
/// Represents the payload for the open_channel message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the OpenChannel1Payload class.
/// </remarks>
public class OpenChannel1Payload : IMessagePayload
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
    /// funding_satoshis is the amount the sender is putting into the channel.
    /// </summary>
    /// <remarks>Amount is used in Satoshis</remarks>
    public LightningMoney FundingAmount { get; set; }

    /// <summary>
    /// push_msat is the amount the sender is pushing to the receiver of the channel.
    /// </summary>
    /// <remarks>Amount is used in Millisatoshis</remarks>
    public LightningMoney PushAmount { get; set; }

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
    public LightningMoney ChannelReserveAmount { get; set; }

    /// <summary>
    /// htlc_minimum_msat indicates the smallest value HTLC this node will accept.
    /// </summary>
    public LightningMoney HtlcMinimumAmount { get; }

    /// <summary>
    /// feerate_per_kw is the fee rate that will be paid for the commitment transaction in
    /// satoshi per 1000-weight
    /// </summary>
    public LightningMoney FeeRatePerKw { get; set; }

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
    /// Only the least-significant bit of channel_flags is currently defined: announce_channel. This indicates whether
    /// the initiator of the funding flow wishes to advertise this channel publicly to the network
    /// </summary>
    public ChannelFlags ChannelFlags { get; set; }

    public OpenChannel1Payload(ChainHash chainHash, ChannelId temporaryChannelId, LightningMoney fundingAmount,
                               LightningMoney pushAmount, LightningMoney dustLimitAmount,
                               LightningMoney maxHtlcValueInFlight, LightningMoney channelReserveAmount,
                               LightningMoney htlcMinimumAmount, LightningMoney feeRatePerKw, ushort toSelfDelay,
                               ushort maxAcceptedHtlcs, PubKey fundingPubKey, PubKey revocationBasepoint,
                               PubKey paymentBasepoint, PubKey delayedPaymentBasepoint, PubKey htlcBasepoint,
                               PubKey firstPerCommitmentPoint, ChannelFlags channelFlags)
    {
        ChainHash = chainHash;
        TemporaryChannelId = temporaryChannelId;
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

    /// <inheritdoc/>
    public async Task SerializeAsync(Stream stream)
    {
        await ChainHash.SerializeAsync(stream);
        await TemporaryChannelId.SerializeAsync(stream);
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FundingAmount.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(PushAmount.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FundingAmount.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(DustLimitAmount.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MaxHtlcValueInFlight.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(ChannelReserveAmount.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(HtlcMinimumAmount.MilliSatoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(FeeRatePerKw.Satoshi));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(ToSelfDelay));
        await stream.WriteAsync(EndianBitConverter.GetBytesBigEndian(MaxAcceptedHtlcs));
        await stream.WriteAsync(FundingPubKey.ToBytes());
        await stream.WriteAsync(RevocationBasepoint.ToBytes());
        await stream.WriteAsync(PaymentBasepoint.ToBytes());
        await stream.WriteAsync(DelayedPaymentBasepoint.ToBytes());
        await stream.WriteAsync(HtlcBasepoint.ToBytes());
        await stream.WriteAsync(FirstPerCommitmentPoint.ToBytes());
        await ChannelFlags.SerializeAsync(stream);
    }

    /// <summary>
    /// Deserializes the payload from a stream.
    /// </summary>
    /// <param name="stream">The stream to deserialize from.</param>
    /// <returns>The deserialized payload.</returns>
    /// <exception cref="PayloadSerializationException">Error deserializing Payload</exception>
    public static async Task<OpenChannel1Payload> DeserializeAsync(Stream stream)
    {
        var bytes = ArrayPool<byte>.Shared.Rent(33);

        try
        {
            var chainHash = await ChainHash.DeserializeAsync(stream);
            var temporaryChannelId = await ChannelId.DeserializeAsync(stream);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ulong)]);
            var fundingSatoshis = LightningMoney.Satoshis(EndianBitConverter.ToUInt64BigEndian(bytes[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ulong)]);
            var pushAmount = LightningMoney.MilliSatoshis(EndianBitConverter.ToUInt64BigEndian(bytes[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ulong)]);
            var dustLimitSatoshis = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(bytes[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ulong)]);
            var maxHtlcValueInFlight = LightningMoney
                .MilliSatoshis(EndianBitConverter.ToUInt64BigEndian(bytes[..sizeof(ulong)]));

            var channelReserveAmount = LightningMoney
                .Satoshis(EndianBitConverter.ToUInt64BigEndian(bytes[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ulong)]);
            var htlcMinimumAmount = LightningMoney
                .MilliSatoshis(EndianBitConverter.ToUInt64BigEndian(bytes[..sizeof(ulong)]));

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(uint)]);
            var feeRatePerKw = LightningMoney.Satoshis(EndianBitConverter.ToUInt32BigEndian(bytes[..sizeof(uint)]));

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ushort)]);
            var toSelfDelay = EndianBitConverter.ToUInt16BigEndian(bytes[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..sizeof(ushort)]);
            var maxAcceptedHtlcs = EndianBitConverter.ToUInt16BigEndian(bytes[..sizeof(ushort)]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..33]);
            var fundingPubKey = new PubKey(bytes[..33]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..33]);
            var revocationBasepoint = new PubKey(bytes[..33]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..33]);
            var paymentBasepoint = new PubKey(bytes[..33]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..33]);
            var delayedPaymentBasepoint = new PubKey(bytes[..33]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..33]);
            var htlcBasepoint = new PubKey(bytes[..33]);

            await stream.ReadExactlyAsync(bytes.AsMemory()[..33]);
            var firstPerCommitmentPoint = new PubKey(bytes[..33]);

            var channelFlags = await ChannelFlags.DeserializeAsync(stream);

            return new OpenChannel1Payload(chainHash, temporaryChannelId, fundingSatoshis, pushAmount,
                                           dustLimitSatoshis, maxHtlcValueInFlight, channelReserveAmount,
                                           htlcMinimumAmount, feeRatePerKw, toSelfDelay, maxAcceptedHtlcs,
                                           fundingPubKey, revocationBasepoint, paymentBasepoint,
                                           delayedPaymentBasepoint, htlcBasepoint, firstPerCommitmentPoint,
                                           channelFlags);
        }
        catch (Exception e)
        {
            throw new PayloadSerializationException("Error deserializing OpenChannel2Payload", e);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(bytes);
        }
    }
}