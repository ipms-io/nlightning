using System.Diagnostics.CodeAnalysis;

namespace NLightning.Infrastructure.Protocol.Tlv.Converters;

using Domain.Protocol.Constants;
using Domain.Protocol.Tlv;
using Domain.Protocol.Tlv.Converters;
using Domain.ValueObjects;

public class NetworksTlvConverter : ITlvConverter<NetworksTlv>
{
    public BaseTlv ConvertToBase(NetworksTlv tlv)
    {
        return tlv;
    }

    public NetworksTlv ConvertFromBase(BaseTlv baseTlv)
    {
        if (baseTlv.Type != TlvConstants.NETWORKS)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length % ChainHash.LENGTH != 0)
        {
            throw new InvalidCastException("Invalid length");
        }

        var chainHashes = new List<ChainHash>();
        // split the Value into 32 bytes chunks and add it to the list
        for (var i = 0; i < baseTlv.Length; i += ChainHash.LENGTH)
        {
            chainHashes.Add(baseTlv.Value[i..(i + ChainHash.LENGTH)]);
        }

        return new NetworksTlv(chainHashes);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertFromBase(BaseTlv tlv)
    {
        return ConvertFromBase(tlv);
    }

    [ExcludeFromCodeCoverage]
    BaseTlv ITlvConverter.ConvertToBase(BaseTlv tlv)
    {
        return ConvertToBase(tlv as NetworksTlv
                             ?? throw new InvalidCastException($"Error converting BaseTlv to {nameof(NetworksTlv)}"));
    }
}