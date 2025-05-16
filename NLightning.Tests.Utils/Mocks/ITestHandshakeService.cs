namespace NLightning.Tests.Utils.Mocks;

using Domain.Transport;

internal interface ITestHandshakeService
{
    (int, ITransport?) PerformStep(byte[] inMessage, out byte[] outMessage);
}