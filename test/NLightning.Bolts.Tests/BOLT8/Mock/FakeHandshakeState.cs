using NLightning.Bolts.BOLT8.Interfaces;
using NLightning.Bolts.BOLT8.Primitives;

namespace NLightning.Bolts.Tests.BOLT8.Mock;

public class FakeHandshakeState : IHandshakeState, ITestHandshakeState
{
    public virtual (int, byte[]?, Transport?) WriteMessageTest(byte[] span, byte[] buffer)
    {
        // Default implementation
        return (0, null, null);
    }

    public virtual (int, byte[]?, Transport?) ReadMessageTest(byte[] buffer, byte[] data)
    {
        // Default implementation
        return (0, null, null);
    }

    public (int, byte[]?, Transport?) WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
    {
        return WriteMessageTest(payload.ToArray(), messageBuffer.ToArray());
    }

    public (int, byte[]?, Transport?) ReadMessage(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
    {
        return ReadMessageTest(message.ToArray(), payloadBuffer.ToArray());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}