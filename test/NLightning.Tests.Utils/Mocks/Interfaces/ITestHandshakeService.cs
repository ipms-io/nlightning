using NLightning.Domain.Transport;

namespace NLightning.Tests.Utils.Mocks.Interfaces;

internal interface ITestHandshakeService
{
    (int, ITransport?) PerformStep(byte[] inMessage, out byte[] outMessage);
}