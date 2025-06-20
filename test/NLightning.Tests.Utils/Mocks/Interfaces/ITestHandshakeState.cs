using NLightning.Infrastructure.Transport.Encryption;

namespace NLightning.Tests.Utils.Mocks.Interfaces;

internal interface ITestHandshakeState
{
    (int, byte[]?, Transport?) WriteMessageTest(byte[] span, byte[] buffer);
    (int, byte[]?, Transport?) ReadMessageTest(byte[] buffer, byte[] data);
}