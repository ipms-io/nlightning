namespace NLightning.Domain.Exceptions;

using Channels.ValueObjects;

public class SignerException : ChannelErrorException
{
    public SignerException(string message, string? peerMessage = null) : base(message, peerMessage)
    {
    }

    public SignerException(string message, Exception innerException, string? peerMessage = null)
        : base(message, innerException, peerMessage)
    {
    }

    public SignerException(string message, ChannelId? channelId, string? peerMessage = null)
        : base(message, channelId, peerMessage)
    {
    }

    public SignerException(string message, ChannelId? channelId, Exception innerException,
                           string? peerMessage = null)
        : base(message, channelId, innerException, peerMessage)
    {
    }
}