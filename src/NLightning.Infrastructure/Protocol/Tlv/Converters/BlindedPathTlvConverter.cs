using NBitcoin;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;

public class BlindedPathTlvConverter : ITlvConverter<BlindedPathTlv>
{
    public BaseTlv ConvertToBase(BlindedPathTlv tlv)
    {
        tlv.Value = tlv.PathKey.ToBytes();

        return tlv;
    }

    public BlindedPathTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.BLINDED_PATH)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length == 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        return new BlindedPathTlv(new PubKey(baseTlv.Value));
    }

    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as BlindedPathTlv
                             ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(BlindedPathTlv)}"));
    }
}