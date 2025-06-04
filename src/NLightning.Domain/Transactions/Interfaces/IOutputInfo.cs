using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Transactions.Enums;

namespace NLightning.Domain.Transactions.Interfaces;

/// <summary>
/// Represents the information needed to construct an output in a transaction.
/// This is the domain representation independent of any specific Bitcoin library.
/// </summary>
public interface IOutputInfo
{
    /// <summary>
    /// Gets the amount of the output.
    /// </summary>
    LightningMoney Amount { get; }

    /// <summary>
    /// Gets the type of the output.
    /// </summary>
    OutputType OutputType { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output once it's created.
    /// </summary>
    TxId? TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction once it's created.
    /// </summary>
    uint? Index { get; set; }
}