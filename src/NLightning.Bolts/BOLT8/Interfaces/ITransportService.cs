namespace NLightning.Bolts.BOLT8.Interfaces;

using Bolts.Interfaces;

public interface ITransportService : IDisposable
{
    Task Initialize();
    void WriteMessage<PayloadType>(IMessage message) where PayloadType : IMessagePayload;
}