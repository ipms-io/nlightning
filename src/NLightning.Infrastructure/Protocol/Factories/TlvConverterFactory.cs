namespace NLightning.Infrastructure.Protocol.Factories;

using Domain.Protocol.Interfaces;
using Domain.Protocol.Tlv;
using Tlv.Converters;

public class TlvConverterFactory : ITlvConverterFactory
{
    private readonly Dictionary<Type, ITlvConverter> _converters = new();

    public TlvConverterFactory()
    {
        RegisterConverters();
    }

    public ITlvConverter<TTlv>? GetConverter<TTlv>() where TTlv : BaseTlv
    {
        return _converters.GetValueOrDefault(typeof(TTlv)) as ITlvConverter<TTlv>;
    }

    private void RegisterConverters()
    {
        _converters.Add(typeof(BlindedPathTlv), new BlindedPathTlvConverter());
        _converters.Add(typeof(ChannelTypeTlv), new ChannelTypeTlvConverter());
        _converters.Add(typeof(FeeRangeTlv), new FeeRangeTlvConverter());
        _converters.Add(typeof(FundingOutputContributionTlv), new FundingOutputContributionTlvConverter());
        _converters.Add(typeof(NetworksTlv), new NetworksTlvConverter());
        _converters.Add(typeof(NextFundingTlv), new NextFundingTlvConverter());
        _converters.Add(typeof(RemoteAddressTlv), new RemoteAddressTlvConverter());
        _converters.Add(typeof(RequireConfirmedInputsTlv), new RequireConfirmedInputsTlvConverter());
        _converters.Add(typeof(ShortChannelIdTlv), new ShortChannelIdTlvConverter());
        _converters.Add(typeof(UpfrontShutdownScriptTlv), new UpfrontShutdownScriptTlvConverter());
    }
}