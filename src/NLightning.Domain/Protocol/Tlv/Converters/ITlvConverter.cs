namespace NLightning.Domain.Protocol.Tlv.Converters;

using Models;

/// <summary>
/// Interface for serializers that handle specific message types
/// </summary>
public interface ITlvConverter
{
    BaseTlv ConvertToBase(BaseTlv tlv);
    BaseTlv ConvertFromBase(BaseTlv baseTlv);
}

/// <summary>
/// Generic version for type safety
/// </summary>
public interface ITlvConverter<TTlv> : ITlvConverter where TTlv : BaseTlv
{
    new TTlv ConvertFromBase(BaseTlv baseTlv);
    BaseTlv ConvertToBase(TTlv tlv);
}