// ReSharper disable PropertyCanBeMadeInitOnly.Global

using NLightning.Domain.Channels.ValueObjects;

namespace NLightning.Infrastructure.Persistence.Entities;

/// <summary>
/// Represents the configuration parameters for a Lightning Network channel.
/// </summary>
public class ChannelConfigEntity
{
    /// <summary>
    /// The unique channel identifier this configuration belongs to.
    /// </summary>
    public required ChannelId ChannelId { get; set; }

    /// <summary>
    /// The minimum number of confirmations required for the funding transaction.
    /// </summary>
    public required uint MinimumDepth { get; set; }

    /// <summary>
    /// The number of blocks that the counterparty's to-self outputs must be delayed.
    /// </summary>
    public required ushort ToSelfDelay { get; set; }

    /// <summary>
    /// The maximum number of HTLCs that can be pending at any given time.
    /// </summary>
    public required ushort MaxAcceptedHtlcs { get; set; }

    /// <summary>
    /// The local minimum value for an output below which it should be considered dust and not included.
    /// </summary>
    public required long LocalDustLimitAmountSats { get; set; }

    /// <summary>
    /// The remote minimum value for an output below which it should be considered dust and not included.
    /// </summary>
    public required long RemoteDustLimitAmountSats { get; set; }

    /// <summary>
    /// The minimum value for an HTLC, expressed in millisatoshis.
    /// </summary>
    public required ulong HtlcMinimumMsat { get; set; }

    /// <summary>
    /// The minimum amount that the counterparty must keep in its balance, if set.
    /// </summary>
    public long? ChannelReserveAmountSats { get; set; }

    /// <summary>
    /// The maximum total value of all HTLCs that can be in-flight at any given time.
    /// </summary>
    public required ulong MaxHtlcAmountInFlight { get; set; }

    /// <summary>
    /// The fee rate in satoshis per kiloweight to use for commitment transactions.
    /// </summary>
    public required long FeeRatePerKwSatoshis { get; set; }

    /// <summary>
    /// Whether anchor outputs are enabled for this channel.
    /// </summary>
    public required bool OptionAnchorOutputs { get; set; }

    /// <summary>
    /// The upfront shutdown script for the local node, if specified.
    /// </summary>
    public byte[]? LocalUpfrontShutdownScript { get; set; }

    /// <summary>
    /// The upfront shutdown script for the remote node, if specified.
    /// </summary>
    public byte[]? RemoteUpfrontShutdownScript { get; set; }

    /// <summary>
    /// Default constructor for EF Core.
    /// </summary>
    internal ChannelConfigEntity()
    {
    }
}