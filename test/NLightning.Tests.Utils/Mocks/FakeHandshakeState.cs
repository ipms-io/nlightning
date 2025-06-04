using System.Diagnostics.CodeAnalysis;
using NBitcoin;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Tests.Utils.Mocks.Interfaces;

namespace NLightning.Tests.Utils.Mocks;

using Infrastructure.Transport.Encryption;
using Infrastructure.Transport.Interfaces;

[ExcludeFromCodeCoverage]
internal class FakeHandshakeState : IHandshakeState, ITestHandshakeState
{
    public CompactPubKey? RemoteStaticPublicKey => new Key().PubKey.ToBytes();

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