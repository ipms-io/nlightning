using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Constants;
using Enums;
using Interfaces;

/// <summary>
/// Tagged field for routing information
/// </summary>
/// <remarks>
/// The routing information is a collection of routing information entries.
/// Each entry contains the public key of the node, the short channel id, the base fee in msat, the fee proportional millionths and the cltv expiry delta.
/// </remarks>
/// <seealso cref="ITaggedField"/>
public sealed class RoutingInfoTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.ROUTING_INFO;
    public RoutingInfoCollection Value { get; }
    public short Length { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionHashTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description Hash</param>
    public RoutingInfoTaggedField(RoutingInfoCollection value)
    {
        Value = value;
        Length = (short)(value.Count * TaggedFieldConstants.ROUTING_INFO_LENGTH + (value.Count * 2));
    }

    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write data
        foreach (var routingInfo in Value)
        {
            bitWriter.WriteBits(routingInfo.PubKey.ToBytes(), 264);
            bitWriter.WriteBits(routingInfo.ShortChannelId.ToByteArray(), 64);
            bitWriter.WriteInt32AsBits(routingInfo.FeeBaseMsat, 32);
            bitWriter.WriteInt32AsBits(routingInfo.FeeProportionalMillionths, 32);
            bitWriter.WriteInt16AsBits(routingInfo.CltvExpiryDelta, 16);
        }

        for (var i = 0; i < Value.Count * 2; i++)
        {
            bitWriter.WriteBit(false);
        }
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        foreach (var routingInfo in Value)
        {
            if (routingInfo.FeeBaseMsat < 0)
            {
                return false;
            }

            if (routingInfo.FeeProportionalMillionths < 0)
            {
                return false;
            }

            if (routingInfo.CltvExpiryDelta < 0)
            {
                return false;
            }
        }

        return true;
    }

    public object GetValue()
    {
        return Value;
    }

    public static RoutingInfoTaggedField FromBitReader(BitReader bitReader, short length)
    {
        var l = length * 5;
        var bitsReadAcc = 0;
        var routingInfos = new RoutingInfoCollection();

        for (var i = 0; i < l && l - bitsReadAcc >= TaggedFieldConstants.ROUTING_INFO_LENGTH; i += TaggedFieldConstants.ROUTING_INFO_LENGTH)
        {
            var pubkeyBytes = new byte[34];
            bitsReadAcc += bitReader.ReadBits(pubkeyBytes, 264);

            var shortChannelBytes = new byte[9];
            bitsReadAcc += bitReader.ReadBits(shortChannelBytes, 64);

            var feeBaseMsat = bitReader.ReadInt32FromBits(32);
            bitsReadAcc += 32;

            var feeProportionalMillionths = bitReader.ReadInt32FromBits(32);
            bitsReadAcc += 32;

            var minFinalCltvExpiry = bitReader.ReadInt16FromBits(16);
            bitsReadAcc += 16;

            routingInfos.Add(new RoutingInfo(new PubKey(pubkeyBytes[..^1]),
                new ShortChannelId(shortChannelBytes[..^1]),
                feeBaseMsat,
                feeProportionalMillionths,
                minFinalCltvExpiry));
        }

        // Skip any extra bits since padding is expected
        var extraBitsToSkip = l - bitsReadAcc;
        if (extraBitsToSkip > 0)
        {
            bitReader.SkipBits(extraBitsToSkip);
        }

        return new RoutingInfoTaggedField(routingInfos);
    }
}