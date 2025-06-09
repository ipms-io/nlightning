namespace NLightning.Domain.Protocol.Interfaces;

using Tlv;

public interface ITlvConverterFactory
{
    ITlvConverter<TTlv>? GetConverter<TTlv>() where TTlv : BaseTlv;
}