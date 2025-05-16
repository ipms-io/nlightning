using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Protocol.Payloads.Interfaces;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

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
/// <seealso cref="ValueObjects.ChannelId"/>
public class TxInitRbfPayload(ChannelId channelId, uint feerate, uint locktime) : IMessagePayload
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