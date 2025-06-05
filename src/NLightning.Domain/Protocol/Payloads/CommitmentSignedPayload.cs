using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Protocol.Payloads;

using Interfaces;

/// <summary>
/// Represents the payload for the commitment_signed message.
/// </summary>
/// <remarks>
/// Initializes a new instance of the CommitmentSignedPayload class.
/// </remarks>
public class CommitmentSignedPayload(
    ChannelId channelId,
    IEnumerable<CompactSignature> htlcSignatures,
    CompactSignature signature) : IChannelMessagePayload
{
    /// <summary>
    /// The channel_id this message refers to
    /// </summary>
    public ChannelId ChannelId { get; } = channelId;

    /// <summary>
    /// The signature for the commitment transaction
    /// </summary>
    public CompactSignature Signature { get; } = signature;

    /// <summary>
    /// Number of HTLCs outputs
    /// </summary>
    public ushort NumHtlcs
    {
        get
        {
            return (ushort)HtlcSignatures.Count();
        }
    }

    /// <summary>
    /// List containing HTLCs signatures
    /// </summary>
    public IEnumerable<CompactSignature> HtlcSignatures { get; set; } = htlcSignatures;
}