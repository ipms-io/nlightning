namespace NLightning.Bolt11.Models.TaggedFields;

using Constants;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Models;
using Domain.Utils;
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
internal sealed class RoutingInfoTaggedField : ITaggedField
{
    public TaggedFieldTypes Type => TaggedFieldTypes.RoutingInfo;
    internal RoutingInfoCollection Value { get; }
    public short Length { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptionHashTaggedField"/> class.
    /// </summary>
    /// <param name="value">The Description Hash</param>
    internal RoutingInfoTaggedField(RoutingInfoCollection value)
    {
        Value = value;
        Length = (short)((value.Count * TaggedFieldConstants.RoutingInfoLength + value.Count * 2) / 5);

        Value.Changed += OnRoutingInfoCollectionChanged;
    }

    /// <inheritdoc/>
    public void WriteToBitWriter(BitWriter bitWriter)
    {
        // Write data
        foreach (var routingInfo in Value)
        {
            bitWriter.WriteBits(routingInfo.CompactPubKey, 264);
            bitWriter.WriteBits(routingInfo.ShortChannelId, 64);
            bitWriter.WriteInt32AsBits(routingInfo.FeeBaseMsat, 32);
            bitWriter.WriteInt32AsBits(routingInfo.FeeProportionalMillionths, 32);
            bitWriter.WriteInt16AsBits(routingInfo.CltvExpiryDelta, 16);
        }

        for (var i = 0; i < Value.Count * 2; i++)
            bitWriter.WriteBit(false);
    }

    /// <inheritdoc/>
    public bool IsValid()
    {
        foreach (var routingInfo in Value)
        {
            if (routingInfo.FeeBaseMsat < 0)
                return false;

            if (routingInfo.FeeProportionalMillionths < 0)
                return false;

            if (routingInfo.CltvExpiryDelta < 0)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Create a new instance of the <see cref="RoutingInfoTaggedField"/> from a <see cref="BitReader"/>
    /// </summary>
    /// <param name="bitReader">The bit reader to read from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <returns>A new instance of the <see cref="RoutingInfoTaggedField"/></returns>
    internal static RoutingInfoTaggedField FromBitReader(BitReader bitReader, short length)
    {
        var l = length * 5;
        var bitsReadAcc = 0;
        var routingInfos = new RoutingInfoCollection();

        for (var i = 0;
             i < l && l - bitsReadAcc >= TaggedFieldConstants.RoutingInfoLength;
             i += TaggedFieldConstants.RoutingInfoLength)
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

            routingInfos.Add(new RoutingInfo(new CompactPubKey(pubkeyBytes[..^1]),
                                             new ShortChannelId(shortChannelBytes[..^1]),
                                             feeBaseMsat,
                                             feeProportionalMillionths,
                                             minFinalCltvExpiry));
        }

        // Skip any extra bits since padding is expected
        var extraBitsToSkip = l - bitsReadAcc;
        if (extraBitsToSkip > 0)
            bitReader.SkipBits(extraBitsToSkip);

        return new RoutingInfoTaggedField(routingInfos);
    }

    private void OnRoutingInfoCollectionChanged(object? sender, EventArgs e)
    {
        Length = (short)((Value.Count * TaggedFieldConstants.RoutingInfoLength + Value.Count * 2) / 5);
    }
}