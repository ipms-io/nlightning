namespace NLightning.Tests.Utils.Mocks;

using Infrastructure.Transport.Encryption;

internal interface ITestHandshakeState
{
    (int, byte[]?, Transport?) WriteMessageTest(byte[] span, byte[] buffer);
    (int, byte[]?, Transport?) ReadMessageTest(byte[] buffer, byte[] data);
}