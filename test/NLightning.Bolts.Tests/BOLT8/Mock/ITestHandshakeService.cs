namespace NLightning.Bolts.Tests.BOLT8.Mock;

using Bolts.BOLT8.Interfaces;

internal interface ITestHandshakeService
{
    (int, ITransport?) PerformStep(byte[] inMessage, out byte[] outMessage);
}