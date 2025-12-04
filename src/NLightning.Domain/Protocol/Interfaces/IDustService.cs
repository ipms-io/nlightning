namespace NLightning.Domain.Protocol.Interfaces;

public interface IDustService
{
    ulong CalculateP2PkhDustLimit();
    ulong CalculateP2ShDustLimit();
    ulong CalculateP2WpkhDustLimit();
    ulong CalculateP2WshDustLimit();
    ulong CalculateUnknownSegwitVersionDustLimit();
}