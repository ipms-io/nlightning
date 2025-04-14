namespace NLightning.Bolts.Tests.BOLT8.Mock;

using Bolts.BOLT8.Interfaces;
using NBitcoin;

internal class FakeHandshakeService : IHandshakeService, ITestHandshakeService
{
    private int _steps = 2;
    private bool _isInitiator;
    private ITransport? _transport;

    public bool IsInitiator => _isInitiator;

    public PubKey RemoteStaticPublicKey => new Key().PubKey;

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