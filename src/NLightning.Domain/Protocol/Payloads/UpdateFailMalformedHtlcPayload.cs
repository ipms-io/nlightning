namespace NLightning.Domain.Protocol.Payloads;

using Channels.ValueObjects;
using Interfaces;

/// <summary>
/// Represents the payload for the update_fail_malformed_htlc message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFailMalformedHtlcPayload class.
/// </remarks>
public class UpdateFailMalformedHtlcPayload(
    ChannelId channelId,
    ushort failureCode,
    ulong id,
    ReadOnlyMemory<byte> sha256OfOnion) : IChannelMessagePayload
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
    /// The sha256 of onion if an onion was received
    /// </summary>
    /// <remarks>
    /// May use an all zero array
    /// </remarks>
    public ReadOnlyMemory<byte> Sha256OfOnion { get; } = sha256OfOnion;

    /// <summary>
    /// The failure code
    /// </summary>
    public ushort FailureCode => failureCode;
}