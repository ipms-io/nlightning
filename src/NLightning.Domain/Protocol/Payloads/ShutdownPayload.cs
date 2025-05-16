using NBitcoin;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the shutdown message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ShutdownPayload class.
/// </remarks>
public class ShutdownPayload(ChannelId channelId, Script scriptPubkey) : IMessagePayload
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
    public Script ScriptPubkey { get; } = scriptPubkey;
}