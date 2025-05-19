namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class ShortChannelIdTlvConverter : ITlvConverter<ShortChannelIdTlv>
{
    public BaseTlv ConvertToBase(ShortChannelIdTlv tlv)
    {
        tlv.Value = tlv.ShortChannelId;

        return tlv;
    }

    public ShortChannelIdTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.SHORT_CHANNEL_ID)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new ShortChannelIdTlv(baseTlv.Value);
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as ShortChannelIdTlv
                             ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(ShortChannelIdTlv)}"));
    }
}