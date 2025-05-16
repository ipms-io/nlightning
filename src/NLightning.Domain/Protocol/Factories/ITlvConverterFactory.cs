namespace NLightning.Domain.Protocol.Factories;

using Models;
using Tlv.Converters;

public interface ITlvConverterFactory
{
    ITlvConverter<TTlv>? GetConverter<TTlv>() where TTlv : BaseTlv;
}