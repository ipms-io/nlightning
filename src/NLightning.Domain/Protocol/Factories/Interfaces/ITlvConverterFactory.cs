namespace NLightning.Domain.Protocol.Factories.Interfaces;

using Models;
using Tlv.Converters;

public interface ITlvConverterFactory
{
    ITlvConverter<TTlv>? GetConverter<TTlv>() where TTlv : BaseTlv;
}