using NLightning.Domain.Bitcoin.Transactions.Enums;
using NLightning.Domain.Bitcoin.Transactions.Interfaces;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Transactions.Outputs;

/// <summary>
/// Represents the information needed to construct a to_remote output in a commitment transaction.
/// This follows the BOLT #3 specification for to_remote outputs.
/// </summary>
public class ToRemoteOutputInfo : IOutputInfo
{
    /// <summary>
    /// Gets the amount of the output.
    /// </summary>
    public LightningMoney Amount { get; }

    /// <summary>
    /// Gets the type of the output.
    /// </summary>
    public OutputType OutputType => OutputType.ToRemote;

    /// <summary>
    /// Gets the remote's payment public key.
    /// </summary>
    public CompactPubKey RemotePaymentPubKey { get; }

    /// <summary>
    /// Gets whether this to_remote output should use anchors.
    /// </summary>
    public bool UseAnchors { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output once it's created.
    /// </summary>
    public TxId? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction once it's created.
    /// </summary>
    public ushort? Index { get; set; }

    /// <summary>
    /// Creates a new instance of ToRemoteOutputInfo.
    /// </summary>
    public ToRemoteOutputInfo(LightningMoney amount, CompactPubKey remotePaymentPubKey, bool useAnchors = false)
    {
        Amount = amount;
        RemotePaymentPubKey = remotePaymentPubKey;
        UseAnchors = useAnchors;
    }
}