using NLightning.Domain.Bitcoin.Transactions.Constants;
using NLightning.Domain.Bitcoin.Transactions.Enums;
using NLightning.Domain.Bitcoin.Transactions.Interfaces;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Transactions.Outputs;

/// <summary>
/// Represents the information needed to construct an anchor output in a commitment transaction.
/// This follows the BOLT #3 specification for anchor outputs when option_anchors is negotiated.
/// </summary>
public class AnchorOutputInfo : IOutputInfo
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
    /// Gets the funding public key used for the anchor.
    /// </summary>
    public CompactPubKey FundingPubKey { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output once it's created.
    /// </summary>
    public TxId? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction once it's created.
    /// </summary>
    public ushort? Index { get; set; }

    /// <summary>
    /// Creates a new instance of AnchorOutputInfo.
    /// </summary>
    public AnchorOutputInfo(CompactPubKey fundingPubKey, bool isLocal)
    {
        Amount = TransactionConstants.AnchorOutputAmount;
        FundingPubKey = fundingPubKey;
        OutputType = isLocal ? OutputType.LocalAnchor : OutputType.RemoteAnchor;
    }
}