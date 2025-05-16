using NBitcoin;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class ChannelTypeTlvConverter : ITlvConverter<ChannelTypeTlv>
{
    public BaseTlv ConvertToBase(ChannelTypeTlv tlv)
    {
        tlv.Value = tlv.ChannelType;
        
        return tlv;
    }

    public ChannelTypeTlv ConvertFromBase(BaseTlv baseTlv)
    {
        var channelTypeTlv = (ChannelTypeTlv)baseTlv;
        
        if (channelTypeTlv.Type != TlvConstants.CHANNEL_TYPE)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (channelTypeTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        channelTypeTlv.ChannelType = baseTlv.Value;
        
        return channelTypeTlv;
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as ChannelTypeTlv 
                             ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(ChannelTypeTlv)}"));
    }
}