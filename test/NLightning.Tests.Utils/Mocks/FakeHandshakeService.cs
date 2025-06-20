using System.Diagnostics.CodeAnalysis;
using NBitcoin;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Tests.Utils.Mocks.Interfaces;

namespace NLightning.Tests.Utils.Mocks;

using Domain.Transport;
using Infrastructure.Transport.Interfaces;

[ExcludeFromCodeCoverage]
internal class FakeHandshakeService : IHandshakeService, ITestHandshakeService
{
    private int _steps = 2;
    private bool _isInitiator;
    private ITransport? _transport;

    public bool IsInitiator => _isInitiator;

    public CompactPubKey? RemoteStaticPublicKey => new Key().PubKey.ToBytes();

    public void SetIsInitiator(bool isInitiator)
    {
        _isInitiator = isInitiator;
    }

    public virtual (int, ITransport?) PerformStep(byte[] inMessage, out byte[] outMessage)
    {
        if (_steps == 2)
        {
            _steps--;
            outMessage = new byte[50];
            return (50, _transport);
        }

        if (_steps == 1)
        {
            _steps--;
            outMessage = new byte[66];

            _transport = new FakeTransport();

            return (66, null);
        }

        throw new InvalidOperationException("There's no more steps to complete");
    }

    public int PerformStep(ReadOnlySpan<byte> inMessage, Span<byte> outMessage, out ITransport? transport)
    {
        (var result, transport) = PerformStep(inMessage.ToArray(), out var outMessageArray);
        outMessageArray.CopyTo(outMessage);
        return result;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}