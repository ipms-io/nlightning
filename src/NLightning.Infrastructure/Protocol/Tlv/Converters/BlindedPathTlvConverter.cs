using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Crypto.ValueObjects;
using Domain.Protocol.Constants;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class BlindedPathTlvConverter : ITlvConverter<BlindedPathTlv>
{
    public BaseTlv ConvertToBase(BlindedPathTlv tlv)
    {
        tlv.Value = tlv.PathKey;

        return tlv;
    }

    public BlindedPathTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.BlindedPath)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new BlindedPathTlv(new CompactPubKey(baseTlv.Value));
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as BlindedPathTlv
                          ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(BlindedPathTlv)}"));
    }
}