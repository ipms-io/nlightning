namespace NLightning.Domain.Transactions.Outputs;

using Channels.ValueObjects;
using Crypto.ValueObjects;

/// <summary>
/// Represents the information needed to construct an offered HTLC output in a commitment transaction.
/// This follows the BOLT #3 specification for offered HTLC outputs.
/// </summary>
public class OfferedHtlcOutputInfo : HtlcOutputInfo
{
    /// <summary>
    /// Creates a new instance of OfferedHtlcOutputInfo.
    /// </summary>
    public OfferedHtlcOutputInfo(Htlc htlc, CompactPubKey localHtlcPubKey, CompactPubKey remoteHtlcPubKey,
                                 CompactPubKey revocationPubKey)
        : base(htlc, localHtlcPubKey, remoteHtlcPubKey, revocationPubKey, true)
    {
    }
}