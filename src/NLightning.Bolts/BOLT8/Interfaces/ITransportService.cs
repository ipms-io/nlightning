using NLightning.Common.Interfaces;

namespace NLightning.Bolts.BOLT8.Interfaces;

public interface ITransportService : IDisposable
{
    Task Initialize();
    void WriteMessage(IMessage message);
}