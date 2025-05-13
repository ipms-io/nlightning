namespace NLightning.Infrastructure.Serialization.Tlv;

using Domain.Protocol.Constants;
using Domain.Protocol.Models;
using Domain.Protocol.Tlvs;
using Interfaces;

public class FundingOutputContributionTlvSerializer : TlvSerializer, ITlvSerializer<FundingOutputContributionTlv>
{
    public FundingOutputContributionTlvSerializer(IValueObjectSerializerFactory valueObjectSerializerFactory)
        : base(valueObjectSerializerFactory)
    {
    }

    public async Task<FundingOutputContributionTlv> DeserializeAsync(Stream stream)
    {
        var baseTlv = await base.DeserializeAsync<BaseTlv>(stream);
        if (baseTlv.Type != TlvConstants.FundingOutputContribution)
        {
            throw new InvalidCastException("Invalid TLV type");
        }

        if (baseTlv.Length != 8) // long (64 bits) is 8 bytes
        {
            throw new InvalidCastException("Invalid length");
        }

        return new FundingOutputContributionTlv(EndianBitConverter.ToInt64BigEndian(baseTlv.Value));
    }

    public async Task SerializeAsync(FundingOutputContributionTlv fundingOutputContributionTlv, Stream stream)
    {
        fundingOutputContributionTlv.Value = EndianBitConverter.GetBytesBigEndian(fundingOutputContributionTlv.Satoshis);
        await base.SerializeAsync(fundingOutputContributionTlv, stream);
    }
}