namespace NLightning.Bolts.BOLT8.Interfaces;

using Bolts.Interfaces;

public interface ITransportService : IDisposable
{
    Task InitializeAsync(TimeSpan networkTimeout);
    Task WriteMessageAsync(IMessage message, CancellationToken cancellationToken = default);

    event EventHandler<MemoryStream>? MessageReceived;
    bool IsInitiator { get; }
    bool IsConnected { get; }
    NBitcoin.PubKey? RemoteStaticPublicKey { get; }
}