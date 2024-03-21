namespace NLightning.Bolts.Tests.BOLT8.Mock;

using Bolts.BOLT8.Primitives;

internal interface ITestHandshakeState
{
    (int, byte[]?, Transport?) WriteMessageTest(byte[] span, byte[] buffer);
    (int, byte[]?, Transport?) ReadMessageTest(byte[] buffer, byte[] data);
}