using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NLightning.Infrastructure.Node.Services;

using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Exceptions;
using Domain.Node.Interfaces;
using Domain.Persistence.Interfaces;
using Domain.Protocol.Constants;
using Domain.Protocol.Interfaces;
using Domain.Protocol.Messages;
using Domain.Protocol.Payloads;

/// <summary>
/// Service for communication with a single peer.
/// </summary>
public class PeerCommunicationService : IPeerCommunicationService
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger<PeerCommunicationService> _logger;
    private readonly IMessageService _messageService;
    private readonly IPingPongService _pingPongService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageFactory _messageFactory;
    private bool _isInitialized;

    /// <inheritdoc />
    public event EventHandler<IMessage?>? MessageReceived;

    /// <inheritdoc />
    public event EventHandler? DisconnectEvent;

    /// <inheritdoc />
    public event EventHandler<Exception>? ExceptionRaised;

    /// <inheritdoc />
    public bool IsConnected => _messageService.IsConnected;

    /// <inheritdoc />
    public CompactPubKey PeerCompactPubKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PeerCommunicationService"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="messageService">The message service.</param>
    /// <param name="messageFactory">The message factory.</param>
    /// <param name="peerCompactPubKey">The peer's public key.</param>
    /// <param name="pingPongService">The ping pong service.</param>
    /// <param name="serviceProvider">The service provider.</param>
    public PeerCommunicationService(ILogger<PeerCommunicationService> logger, IMessageService messageService,
                                    IMessageFactory messageFactory, CompactPubKey peerCompactPubKey,
                                    IPingPongService pingPongService, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _messageService = messageService;
        _messageFactory = messageFactory;
        PeerCompactPubKey = peerCompactPubKey;
        _pingPongService = pingPongService;
        _serviceProvider = serviceProvider;

        _messageService.OnMessageReceived += HandleMessageReceived;
        _messageService.OnExceptionRaised += HandleExceptionRaised;
        _pingPongService.DisconnectEvent += HandleExceptionRaised;
    }

    /// <inheritdoc />
    public async Task InitializeAsync(TimeSpan networkTimeout)
    {
        // Always send an init message upon connection
        _logger.LogTrace("Sending init message to peer {peer}", PeerCompactPubKey);
        var initMessage = _messageFactory.CreateInitMessage();
        await _messageService.SendMessageAsync(initMessage, _cancellationTokenSource.Token);

        // Wait for an init message
        _logger.LogTrace("Waiting for init message from peer {peer}", PeerCompactPubKey);

        // Set timeout to close connection if the other peer doesn't send an init message
        _ = Task.Delay(networkTimeout, _cancellationTokenSource.Token).ContinueWith(task =>
        {
            if (!task.IsCanceled && !_isInitialized)
            {
                RaiseException(
                    new ConnectionException($"Peer {PeerCompactPubKey} did not send init message after timeout"));
            }
        });

        if (!_messageService.IsConnected)
        {
            throw new ConnectionException($"Failed to connect to peer {PeerCompactPubKey}");
        }

        // Set up ping service to keep connection alive
        SetupPingPongService();
    }

    /// <inheritdoc />
    public async Task SendMessageAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            await _messageService.SendMessageAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            RaiseException(new ConnectionException($"Failed to send message to peer {PeerCompactPubKey}", ex));
        }
    }

    /// <inheritdoc />
    public void Disconnect()
    {
        _logger.LogInformation("Disconnecting peer {peer}", PeerCompactPubKey);
        _cancellationTokenSource.Cancel();
        _messageService.Dispose();

        DisconnectEvent?.Invoke(this, EventArgs.Empty);
    }

    private void SetupPingPongService()
    {
        _pingPongService.OnPingMessageReady += HandlePingMessageReady;
        _pingPongService.OnPongReceived += HandlePongReceived;

        // Setup Ping to keep connection alive
        _ = _pingPongService.StartPingAsync(_cancellationTokenSource.Token).ContinueWith(task =>
        {
            if (task.IsFaulted)
            {
                RaiseException(new ConnectionException($"Failed to start ping service for peer {PeerCompactPubKey}",
                                                       task.Exception));
            }
        });

        _logger.LogInformation("Ping service started for peer {peer}", PeerCompactPubKey);
    }

    private void HandlePongReceived(object? sender, EventArgs e)
    {
        using var scope = _serviceProvider.CreateScope();
        using var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        uow.PeerDbRepository.UpdatePeerLastSeenAsync(PeerCompactPubKey).GetAwaiter().GetResult();
        uow.SaveChanges();
    }

    private void HandlePingMessageReady(object? sender, IMessage pingMessage)
    {
        // We can only send ping messages if the peer is initialized
        if (!_isInitialized)
            return;

        try
        {
            _messageService.SendMessageAsync(pingMessage, _cancellationTokenSource.Token).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            RaiseException(new ConnectionException($"Failed to send ping message to peer {PeerCompactPubKey}", ex));
        }
    }

    private void HandleMessageReceived(object? sender, IMessage? message)
    {
        if (message is null)
        {
            return;
        }

        if (!_isInitialized && message.Type == MessageTypes.Init)
        {
            _isInitialized = true;
        }

        // Forward the message to subscribers
        MessageReceived?.Invoke(this, message);

        // Handle ping messages internally
        if (_isInitialized && message.Type == MessageTypes.Ping)
        {
            _ = HandlePingAsync(message);
        }
        else if (_isInitialized && message.Type == MessageTypes.Pong)
        {
            _pingPongService.HandlePong(message);
        }
    }

    private async Task HandlePingAsync(IMessage pingMessage)
    {
        var pongMessage = _messageFactory.CreatePongMessage(pingMessage);
        await _messageService.SendMessageAsync(pongMessage);
    }

    private void HandleExceptionRaised(object? sender, Exception e)
    {
        RaiseException(e);
    }

    private void RaiseException(Exception exception)
    {
        var mustDisconnect = false;
        if (exception is ErrorException errorException)
        {
            ChannelId? channelId = null;
            var message = errorException.Message;

            if (errorException is ChannelErrorException channelErrorException)
            {
                channelId = channelErrorException.ChannelId;
                if (!string.IsNullOrWhiteSpace(channelErrorException.PeerMessage))
                    message = channelErrorException.PeerMessage;
            }

            _messageService.SendMessageAsync(new ErrorMessage(new ErrorPayload(channelId, message)));
            mustDisconnect = true;
        }
        else if (exception is WarningException warningException)
        {
            ChannelId? channelId = null;
            var message = warningException.Message;

            if (warningException is ChannelWarningException channelWarningException)
            {
                channelId = channelWarningException.ChannelId;
                if (!string.IsNullOrWhiteSpace(channelWarningException.PeerMessage))
                    message = channelWarningException.PeerMessage;
            }

            _messageService.SendMessageAsync(new WarningMessage(new ErrorPayload(channelId, message)));
        }

        _logger.LogError(exception, "Exception occurred with peer {peer}. {exceptionMessage}", PeerCompactPubKey,
                         exception.Message);

        // Forward the exception to subscribers
        ExceptionRaised?.Invoke(this, exception);

        // Disconnect if not already disconnecting
        if (mustDisconnect && !_cancellationTokenSource.IsCancellationRequested)
            Disconnect();
    }
}