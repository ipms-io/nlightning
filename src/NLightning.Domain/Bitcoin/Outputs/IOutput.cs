using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Outputs;

public interface IOutput
{
    /// <summary>
    /// Gets the amount of the output.
    /// </summary>
    LightningMoney Amount { get; }

    /// <summary>
    /// Gets the scriptPubKey of the output.
    /// </summary>
    public BitcoinScript BitcoinScriptPubKey { get; }

    /// <summary>
    /// Gets the redeemScript of the output, if applicable.
    /// </summary>
    public BitcoinScript RedeemBitcoinScript { get; }

    /// <summary>
    /// Gets or sets the transaction ID of the output.
    /// </summary>
    public TxId TransactionId { get; set; }

    /// <summary>
    /// Gets or sets the index of the output in the transaction.
    /// </summary>
    public uint Index { get; set; }

    // TxOut ToTxOut();
    // ScriptCoin ToCoin();
    int CompareTo(IOutput? other);
}