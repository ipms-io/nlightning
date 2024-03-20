namespace NLightning.Bolts.BOLT8.Interfaces;

using Bolts.Interfaces;

public interface ITransportService : IDisposable
{
    Task Initialize();
    void WriteMessage<PayloadType>(IMessage<PayloadType> message) where PayloadType : IMessagePayload;
}