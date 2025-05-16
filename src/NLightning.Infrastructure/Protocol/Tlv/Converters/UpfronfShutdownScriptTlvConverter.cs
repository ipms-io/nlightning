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
        var upfrontShutdownScriptTlv = (UpfrontShutdownScriptTlv)baseTlv;

        if (upfrontShutdownScriptTlv.Type != TlvConstants.UPFRONT_SHUTDOWN_SCRIPT)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (upfrontShutdownScriptTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }
        
        upfrontShutdownScriptTlv.ShutdownScriptPubkey = new Script(upfrontShutdownScriptTlv.Value);
        
        return upfrontShutdownScriptTlv;
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