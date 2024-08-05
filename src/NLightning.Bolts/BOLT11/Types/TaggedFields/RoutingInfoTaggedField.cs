using System.Diagnostics.CodeAnalysis;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using Common.BitUtils;
using Enums;

/// <summary>
/// Tagged field for routing information
/// </summary>
/// <remarks>
/// The routing information is a collecion of routing information entries.
/// Each entry contains the public key of the node, the short channel id, the base fee in msat, the fee proportional millionths and the cltv expiry delta.
/// </remarks>
/// <seealso cref="RoutingInfo"/>
/// <seealso cref="RoutingInfoCollection"/>
/// <seealso cref="BaseTaggedField{T}"/>
public sealed class RoutingInfoTaggedField : BaseTaggedField<RoutingInfoCollection>
{
    /// <summary>
    /// The length of a single routing information entry in bits
    /// </summary>
    /// <remarks>
    /// The routing information entry is 264 + 64 + 32 + 32 + 16 = 408 bits long
    /// </remarks>
    private const int ROUTING_INFO_LENGTH = 408;

    /// <summary>
    /// Constructor for RoutingInfoTaggedField from a BitReader and a length
    /// </summary>
    /// <param name="bitReader">The BitReader to read the data from</param>
    /// <param name="length">The length of the tagged field</param>
    /// <remarks>
    /// This constructor is used to create a RoutingInfoTaggedField from a BitReader and a length.
    /// The Value property is set to the decoded value.
    /// </remarks>
    /// <seealso cref="BitReader"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public RoutingInfoTaggedField(BitReader bitReader, short length) : base(TaggedFieldTypes.RoutingInfo)
    {
        LengthInBits = length;
        var l = length * 5;
        var bitsReadAcc = 0;
        var dataBytes = new List<byte>();

        for (var i = 0; i < l && l - bitsReadAcc >= ROUTING_INFO_LENGTH; i += ROUTING_INFO_LENGTH)
        {
            var buffer = new byte[33];
            bitsReadAcc += bitReader.ReadBits(buffer, 264);
            dataBytes.AddRange(buffer);

            buffer = new byte[8];
            bitsReadAcc += bitReader.ReadBits(buffer, 64);
            dataBytes.AddRange(buffer);

            var feeBaseMsat = bitReader.ReadInt32FromBits(32);
            bitsReadAcc += 32;
            dataBytes.AddRange(EndianBitConverter.GetBytesBE(feeBaseMsat));

            var feeProportionalMillionths = bitReader.ReadInt32FromBits(32);
            bitsReadAcc += 32;
            dataBytes.AddRange(EndianBitConverter.GetBytesBE(feeProportionalMillionths));

            var minFinalCltvExpiry = bitReader.ReadInt16FromBits(16);
            bitsReadAcc += 16;
            dataBytes.AddRange(EndianBitConverter.GetBytesBE(minFinalCltvExpiry));
        }

        // Skip any extra bits since padding is expected
        var extraBitsToSkip = l - bitsReadAcc;
        if (extraBitsToSkip > 0)
        {
            bitReader.SkipBits(extraBitsToSkip);
        }

        Data = [.. dataBytes];
        Value = Decode(Data);

        // Subscribe to collection changes
        Value.CollectionChanged += (sender, args) => Data = Encode(Value);
    }

    /// <summary>
    /// Constructor for RoutingInfoTaggedField from a value
    /// </summary>
    /// <param name="value">A RoutingInfoList containing routing information</param>
    /// <remarks>
    /// This constructor is used to create a RoutingInfoTaggedField from a value.
    /// The Data property is set to the encoded value.
    /// </remarks>
    /// <seealso cref="RoutingInfoCollection"/>
    /// <seealso cref="RoutingInfo"/>
    /// <seealso cref="BaseTaggedField{T}"/>
    /// <seealso cref="TaggedFieldTypes"/>
    [SetsRequiredMembers]
    public RoutingInfoTaggedField(RoutingInfoCollection value) : base(TaggedFieldTypes.RoutingInfo, value)
    {
        Value.CollectionChanged += (sender, args) => Data = Encode(Value);
    }

    /// <inheritdoc/>
    public override bool IsValid()
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

    /// <inheritdoc/>
    /// <returns>RoutingInfoList</returns>
    /// <seealso cref="RoutingInfoCollection"/>
    /// <seealso cref="RoutingInfo"/>
    protected override RoutingInfoCollection Decode(byte[] data)
    {
        var routingInfos = new RoutingInfoCollection();
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            routingInfos.Add(new RoutingInfo(new PubKey(reader.ReadBytes(33)),
                                      new ShortChannelId(reader.ReadBytes(8)),
                                      EndianBitConverter.ToInt32BE(reader.ReadBytes(4)),
                                      EndianBitConverter.ToInt32BE(reader.ReadBytes(4)),
                                      EndianBitConverter.ToInt16BE(reader.ReadBytes(2))));
        }

        return routingInfos;
    }

    /// <inheritdoc/>
    /// <returns>byte[] representation of the Value</returns>
    /// <seealso cref="RoutingInfoCollection"/>
    protected override byte[] Encode(RoutingInfoCollection value)
    {
        var dataBytes = new List<byte>();
        foreach (var routingInfo in value)
        {
            dataBytes.AddRange(routingInfo.PubKey.ToBytes());
            dataBytes.AddRange(routingInfo.ShortChannelId.ToByteArray());
            dataBytes.AddRange(EndianBitConverter.GetBytesBE(routingInfo.FeeBaseMsat));
            dataBytes.AddRange(EndianBitConverter.GetBytesBE(routingInfo.FeeProportionalMillionths));
            dataBytes.AddRange(EndianBitConverter.GetBytesBE(routingInfo.CltvExpiryDelta));
        }

        return AccountForPaddingWhenEncoding([.. dataBytes]);
    }
}