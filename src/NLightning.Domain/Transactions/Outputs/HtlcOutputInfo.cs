using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Transactions.Enums;
using NLightning.Domain.Transactions.Interfaces;

namespace NLightning.Domain.Transactions.Outputs;

/// <summary>
/// Base class for HTLC output information.
/// </summary>
public abstract class HtlcOutputInfo : IOutputInfo
{
    /// <summary>
    /// Gets the amount of the output.
    /// </summary>
    public LightningMoney Amount { get; }

    /// <summary>
    /// Gets the type of the output.
    /// </summary>
    public OutputType OutputType { get; }

    /// <summary>
    /// Gets the HTLC this output is based on.
    /// </summary>
    public Htlc Htlc { get; }

    /// <summary>
    /// Gets the payment hash for this HTLC.
    /// </summary>
    public Hash PaymentHash => Htlc.PaymentHash;

    /// <summary>
    /// Gets the CLTV expiry for this HTLC.
    /// </summary>
    public uint CltvExpiry => Htlc.CltvExpiry;

    /// <summary>
    /// Gets the revocation public key.
    /// </summary>
    public CompactPubKey RevocationPubKey { get; }

    /// <summary>
    /// Gets the local HTLC public key.
    /// </summary>
    public CompactPubKey LocalHtlcPubKey { get; }

    /// <summary>
    /// Gets the remote HTLC public key.
    /// </summary>
    public CompactPubKey RemoteHtlcPubKey { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output once it's created.
    /// </summary>
    public TxId? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction once it's created.
    /// </summary>
    public uint? Index { get; set; }

    /// <summary>
    /// Creates a new instance of HtlcOutputInfo.
    /// </summary>
    protected HtlcOutputInfo(Htlc htlc, CompactPubKey localHtlcPubKey, CompactPubKey remoteHtlcPubKey,
                             CompactPubKey revocationPubKey, bool isOffered)
    {
        Htlc = htlc;
        Amount = htlc.Amount;
        RevocationPubKey = revocationPubKey;
        LocalHtlcPubKey = localHtlcPubKey;
        RemoteHtlcPubKey = remoteHtlcPubKey;
        OutputType = isOffered ? OutputType.OfferedHtlc : OutputType.ReceivedHtlc;
    }
}