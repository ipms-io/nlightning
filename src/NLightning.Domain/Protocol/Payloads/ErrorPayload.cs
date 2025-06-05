using System.Text;
using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;

/// <summary>
/// Represents an error payload.
/// </summary>
/// <remarks>
/// An error payload is used to communicate an error to the other party.
/// </remarks>
/// <seealso cref="ErrorMessage"/>
public class ErrorPayload : IMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    /// <remarks>
    /// The channel id is used to identify the channel the error is related to.
    /// </remarks>
    public ChannelId ChannelId { get; } = ChannelId.Zero;

    /// <summary>
    /// The error data.
    /// </summary>
    /// <remarks>
    /// The error data is used to communicate the error.
    /// </remarks>
    public byte[]? Data { get; }

    public ErrorPayload(byte[] data)
    {
        Data = data;
    }
    public ErrorPayload(ChannelId? channelId, byte[] data) : this(data)
    {
        if (channelId.HasValue)
            ChannelId = channelId.Value;
    }
    public ErrorPayload(ChannelId? channelId, string message) : this(channelId, Encoding.UTF8.GetBytes(message))
    { }
    public ErrorPayload(string message) : this(Encoding.UTF8.GetBytes(message))
    { }
}