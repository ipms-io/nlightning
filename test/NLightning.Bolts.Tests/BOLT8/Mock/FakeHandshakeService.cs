using NLightning.Bolts.BOLT8.Interfaces;

namespace NLightning.Bolts.Tests.BOLT8.Mock;

internal class FakeHandshakeService : IHandshakeService, ITestHandshakeService
{
    private int _steps = 2;
    private bool _isInitiator;
    private ITransport? _transport;

    public bool IsInitiator => _isInitiator;

    public ITransport? Transport => _transport;

    public void SetIsInitiator(bool isInitiator)
    {
        _isInitiator = isInitiator;
    }

    public void SetTransport(ITransport transport)
    {
        _transport = transport;
    }

    public virtual int PerformStep(byte[] inMessage, out byte[] outMessage)
    {
        if (_steps == 2)
        {
            _steps--;
            outMessage = new byte[50];
            return 50;
        }
        else if (_steps == 1)
        {
            _steps--;
            outMessage = new byte[66];

            _transport = new FakeTransport();

            return 66;
        }

        throw new InvalidOperationException("There's no more steps to complete");
    }

    public int PerformStep(ReadOnlySpan<byte> inMessage, Span<byte> outMessage)
    {
        var result = PerformStep(inMessage.ToArray(), out var outMessageArray);
        outMessageArray.CopyTo(outMessage);
        return result;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}