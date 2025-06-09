namespace NLightning.Domain.Protocol.Payloads;

using Channels.ValueObjects;
using Interfaces;
using Messages;

/// <summary>
/// Represents the payload for the tx_init_rbf message.
/// </summary>
/// <remarks>
/// The tx_init_rbf message is used to initialize a new RBF transaction.
/// </remarks>
/// 
/// <remarks>
/// Initializes a new instance of the TxInitRbfPayload class.
/// </remarks>
/// <param name="channelId">The channel ID.</param>
/// <param name="feerate">The feerate.</param>
/// <param name="locktime">The locktime.</param>
/// <seealso cref="TxInitRbfMessage"/>
/// <seealso cref="Channels.ValueObjects.ChannelId"/>
public class TxInitRbfPayload(ChannelId channelId, uint feerate, uint locktime) : IChannelMessagePayload
{
    /// <summary>
    /// The channel ID.
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The locktime.
    /// </summary>
    public uint Locktime { get; } = locktime;

    /// <summary>
    /// The feerate.
    /// </summary>
    public uint Feerate { get; } = feerate;
}