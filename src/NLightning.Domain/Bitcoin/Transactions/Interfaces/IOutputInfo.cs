using NLightning.Domain.Bitcoin.Transactions.Enums;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Transactions.Interfaces;

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
    ushort? Index { get; set; }
}