namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
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
        var shortChannelIdTlv = (ShortChannelIdTlv)baseTlv;

        if (shortChannelIdTlv.Type != TlvConstants.SHORT_CHANNEL_ID)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (shortChannelIdTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }
        
        shortChannelIdTlv.ShortChannelId = shortChannelIdTlv.Value;
        
        return shortChannelIdTlv;
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