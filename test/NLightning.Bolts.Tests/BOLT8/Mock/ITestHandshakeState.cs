using NLightning.Bolts.BOLT8.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Mock;

internal interface ITestHandshakeState
{
    (int, byte[]?, Transport?) WriteMessageTest(byte[] span, byte[] buffer);
    (int, byte[]?, Transport?) ReadMessageTest(byte[] buffer, byte[] data);
}