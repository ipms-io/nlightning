namespace NLightning.Bolts.BOLT1.Interfaces;

using BOLT8.Interfaces;

public interface IMessageServiceFactory
{
    IMessageService CreateMessageService(ITransportService transportService);
}