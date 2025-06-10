namespace NLightning.Domain.Protocol.Payloads;

using Bitcoin.ValueObjects;
using Channels.ValueObjects;
using Interfaces;

/// <summary>
/// Represents the payload for the shutdown message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ShutdownPayload class.
/// </remarks>
public class ShutdownPayload(ChannelId channelId, BitcoinScript scriptPubkey) : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// len is the scriptpubkey length
    /// </summary>
    public ushort ScriptPubkeyLen { get; } = (ushort)scriptPubkey.Length;

    /// <summary>
    /// The scriptpubkey to send the closing funds to
    /// </summary>
    public BitcoinScript ScriptPubkey { get; } = scriptPubkey;
}