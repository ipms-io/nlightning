namespace NLightning.Bolts.BOLT1.Factories;

using BOLT8.Interfaces;
using Interfaces;
using Services;

public sealed class MessageServiceFactory : IMessageServiceFactory
{
    public IMessageService CreateMessageService(ITransportService transportService)
    {
        return new MessageService(transportService);
    }
}