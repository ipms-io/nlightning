using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the update_fail_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFailHtlcPayload class.
/// </remarks>
public class UpdateFailHtlcPayload(ChannelId channelId, ulong id, ReadOnlyMemory<byte> reason) : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The htlc id
    /// </summary>
    public ulong Id { get; } = id;

    /// <summary>
    /// The length of the reason
    /// </summary>
    public ushort Len => (ushort)Reason.Length;

    /// <summary>
    /// The reason for failure
    /// </summary>
    public ReadOnlyMemory<byte> Reason { get; } = reason;
}