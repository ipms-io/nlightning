using NBitcoin;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class UpfrontShutdownScriptTlvConverter : ITlvConverter<UpfrontShutdownScriptTlv>
{
    public BaseTlv ConvertToBase(UpfrontShutdownScriptTlv tlv)
    {
        tlv.Value = tlv.ShutdownScriptPubkey.ToBytes();
        
        return tlv;
    }

    public UpfrontShutdownScriptTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }
        
        return new UpfrontShutdownScriptTlv(new Script(baseTlv.Value));
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as UpfrontShutdownScriptTlv 
                             ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(UpfrontShutdownScriptTlv)}"));
    }
}