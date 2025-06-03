using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;
using Messages;
using ValueObjects;

/// <summary>
/// Represents a tx_signatures payload.
/// </summary>
/// <remarks>
/// The tx_signatures payload signals the provision of transaction signatures.
/// </remarks>
/// <seealso cref="TxSignaturesMessage"/>
/// <seealso cref="Channels.ValueObjects.ChannelId"/>
/// <seealso cref="Witness"/>
public class TxSignaturesPayload : IChannelMessagePayload
{
    /// <summary>
    /// The channel id.
    /// </summary>
    public ChannelId ChannelId { get; }

    /// <summary>
    /// The transaction id.
    /// </summary>
    public byte[] TxId { get; }

    /// <summary>
    /// The witnesses.
    /// </summary>
    public List<Witness> Witnesses { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TxSignaturesPayload"/> class.
    /// </summary>
    /// <param name="channelId">The channel id.</param>
    /// <param name="txId">The transaction id.</param>
    /// <param name="witnesses">The witnesses.</param>
    /// <exception cref="ArgumentException">TxId must be 32 bytes</exception>
    public TxSignaturesPayload(ChannelId channelId, byte[] txId, List<Witness> witnesses)
    {
        if (txId.Length != 32)
            throw new ArgumentException("TxId must be 32 bytes", nameof(txId));

        ChannelId = channelId;
        TxId = txId;
        Witnesses = witnesses;
    }
}