using NLightning.Bolts.BOLT8.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Mock;

internal interface ITestHandshakeService
{
    int PerformStep(byte[] inMessage, out byte[] outMessage);
}