using System.Diagnostics.CodeAnalysis;
using System.Text;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

public sealed class RoutingInfoTaggedField : BaseTaggedField<List<RoutingInfo>>
{
    [SetsRequiredMembers]
    public RoutingInfoTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.RoutingInfo, length)
    {
        var lengthBits = Data.Length * 8;
        if (lengthBits % 408 != 0)
        {
            throw new ArgumentException("RoutingInfo length must be a multiple of 408", nameof(length));
        }

        var routingInfos = new List<RoutingInfo>();

        for (var i = 0; i < lengthBits; i += 408)
        {
            var pubkey = new byte[33];
            buffer.ReadBits(pubkey, 264);

            var shortChannelId = new byte[8];
            buffer.ReadBits(shortChannelId, 64);

            routingInfos.Add(new RoutingInfo(new PubKey(pubkey),
                                              Encoding.UTF8.GetString(shortChannelId),
                                              buffer.ReadInt32FromBits(32),
                                              buffer.ReadInt32FromBits(32),
                                              buffer.ReadInt16FromBits(16)));
        }

        Value = routingInfos;
    }

    protected override List<RoutingInfo> Decode(byte[] data)
    {
        throw new NotImplementedException();
    }

    public override bool IsValid()
    {
        foreach (var routingInfo in Value)
        {
            if (routingInfo.PubKey == null)
            {
                return false;
            }

            if (routingInfo.ShortChannelId.Length != 8)
            {
                return false;
            }

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
}