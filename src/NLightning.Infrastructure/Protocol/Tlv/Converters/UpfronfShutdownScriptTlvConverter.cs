using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Bitcoin.ValueObjects;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;

public class UpfrontShutdownScriptTlvConverter : ITlvConverter<UpfrontShutdownScriptTlv>
{
    public BaseTlv ConvertToBase(UpfrontShutdownScriptTlv tlv)
    {
        tlv.Value = tlv.ShutdownScriptPubkey;

        return tlv;
    }

    public UpfrontShutdownScriptTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.UpfrontShutdownScript)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        return new UpfrontShutdownScriptTlv(new BitcoinScript(baseTlv.Value));
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as UpfrontShutdownScriptTlv
                          ?? throw new InvalidCastException(
                                 $"Error converting BaseTlv to {nameof(UpfrontShutdownScriptTlv)}"));
    }
}