namespace NLightning.Bolts.Tests.BOLT8.Mock;

using Bolts.BOLT8.Interfaces;

public class FakeTransport : ITransport
{
    public int ReadMessageLength(ReadOnlySpan<byte> lc)
    {
        throw new NotImplementedException();
    }

    public int ReadMessagePayload(ReadOnlySpan<byte> message, Span<byte> payloadBuffer)
    {
        throw new NotImplementedException();
    }

    public int WriteMessage(ReadOnlySpan<byte> payload, Span<byte> messageBuffer)
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}