using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;

namespace NLightning.Domain.Transactions.Outputs;

/// <summary>
/// Represents the information needed to construct a received HTLC output in a commitment transaction.
/// This follows the BOLT #3 specification for received HTLC outputs.
/// </summary>
public class ReceivedHtlcOutputInfo : HtlcOutputInfo
{
    /// <summary>
    /// Creates a new instance of ReceivedHtlcOutputInfo.
    /// </summary>
    public ReceivedHtlcOutputInfo(Htlc htlc, CompactPubKey localHtlcPubKey, CompactPubKey remoteHtlcPubKey,
                                  CompactPubKey revocationPubKey)
        : base(htlc, localHtlcPubKey, remoteHtlcPubKey, revocationPubKey, false)
    {
    }
}
