namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using ValueObjects;

/// <summary>
/// Represents the payload for the update_fee message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the UpdateFeePayload class.
/// </remarks>
public class UpdateFeePayload(ChannelId channelId, uint feeratePerKw) : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The fee rate per kw
    /// </summary>
    public uint FeeratePerKw { get; } = feeratePerKw;
}