using System.Diagnostics.CodeAnalysis;
using System.Text;
using NBitcoin;

namespace NLightning.Bolts.BOLT11.Types.TaggedFields;

using BOLT11.Enums;

public sealed class RoutingInfoTaggedField : BaseTaggedField<List<RoutingInfo>>
{
    private const int ROUTING_INFO_LENGTH = 408; // 264 + 64 + 32 + 32 + 16 = 408

    [SetsRequiredMembers]
    public RoutingInfoTaggedField(BitReader buffer, int length) : base(TaggedFieldTypes.RoutingInfo, length)
    {
        var lengthBits = length * 5;
        var routingInfos = new List<RoutingInfo>();
        var bitsReadAcc = 0;

        for (var i = 0; i < lengthBits && lengthBits - bitsReadAcc >= ROUTING_INFO_LENGTH; i += ROUTING_INFO_LENGTH)
        {
            var pubkey = new byte[33];
            bitsReadAcc += buffer.ReadBits(pubkey, 264);

            var shortChannelId = new byte[8];
            bitsReadAcc += buffer.ReadBits(shortChannelId, 64);

            routingInfos.Add(new RoutingInfo(new PubKey(pubkey),
                                             Encoding.UTF8.GetString(shortChannelId),
                                             buffer.ReadInt32FromBits(32),
                                             buffer.ReadInt32FromBits(32),
                                             buffer.ReadInt16FromBits(16)));

            // Add the 80 bits read
            bitsReadAcc += 80;
        }

        Value = routingInfos;

        // Skip any extra bits since padding is expected
        var extraBitsToSkip = lengthBits - bitsReadAcc;
        if (extraBitsToSkip > 0)
        {
            buffer.SkipBits(extraBitsToSkip);
        }
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