using System.Diagnostics.CodeAnalysis;

namespace NLightning.Domain.Exceptions;

using ValueObjects;

/// <summary>
/// Represents an exception that is thrown when a channel error occurs.
/// </summary>
/// <remarks>
/// We usually want to close the connection when this exception is thrown.
/// </remarks>
[ExcludeFromCodeCoverage]
public class ChannelWarningException : ErrorException
{
    public ChannelId? ChannelId { get; }
    public string? PeerMessage { get; }

    public ChannelWarningException(string message, string? peerMessage = null) : base(message)
    {
        PeerMessage = peerMessage;
    }

    public ChannelWarningException(string message, Exception innerException, string? peerMessage = null) : base(message)
    {
        PeerMessage = peerMessage;
    }

    public ChannelWarningException(string message, ChannelId? channelId, string? peerMessage = null) : base(message)
    {
        ChannelId = channelId;
        PeerMessage = peerMessage;
    }
    public ChannelWarningException(string message, ChannelId? channelId, Exception innerException,
                                   string? peerMessage = null)
        : base(message)
    {
        ChannelId = channelId;
        PeerMessage = peerMessage;
    }
}