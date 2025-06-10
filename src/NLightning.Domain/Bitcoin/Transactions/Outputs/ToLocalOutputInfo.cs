using NLightning.Domain.Bitcoin.Transactions.Enums;
using NLightning.Domain.Bitcoin.Transactions.Interfaces;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Transactions.Outputs;

/// <summary>
/// Represents the information needed to construct a to_local output in a commitment transaction.
/// This follows the BOLT #3 specification for to_local outputs.
/// </summary>
public class ToLocalOutputInfo : IOutputInfo
{
    /// <summary>
    /// Gets the amount of the output.
    /// </summary>
    public LightningMoney Amount { get; }

    /// <summary>
    /// Gets the type of the output.
    /// </summary>
    public OutputType OutputType => OutputType.ToLocal;

    /// <summary>
    /// Gets the remote's revocation public key.
    /// </summary>
    public CompactPubKey RevocationPubKey { get; }

    /// <summary>
    /// Gets the local's delayed payment public key.
    /// </summary>
    public CompactPubKey LocalDelayedPaymentPubKey { get; }

    /// <summary>
    /// Gets the CSV delay for the to_local output.
    /// </summary>
    public ushort ToSelfDelay { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output once it's created.
    /// </summary>
    public TxId? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction once it's created.
    /// </summary>
    public uint? Index { get; set; }

    /// <summary>
    /// Creates a new instance of ToLocalOutputInfo.
    /// </summary>
    public ToLocalOutputInfo(LightningMoney amount, CompactPubKey localDelayedPaymentPubKey,
                             CompactPubKey revocationPubKey, ushort toSelfDelay)
    {
        Amount = amount;
        RevocationPubKey = revocationPubKey;
        LocalDelayedPaymentPubKey = localDelayedPaymentPubKey;
        ToSelfDelay = toSelfDelay;
    }
}