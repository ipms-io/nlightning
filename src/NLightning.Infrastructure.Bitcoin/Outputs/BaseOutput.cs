using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Comparers;
using Domain.Bitcoin.Outputs;
using Domain.Bitcoin.ValueObjects;
using Domain.Money;

/// <summary>
/// Represents a transaction output.
/// </summary>
public abstract class BaseOutput : IOutput
{
    internal Script ScriptPubKey;
    protected uint256 TxIdHash;
    protected Script RedeemScript;

    /// <inheritdoc />
    public LightningMoney Amount { get; set; }

    /// <inheritdoc />
    public BitcoinScript BitcoinScriptPubKey
    {
        get => ScriptPubKey.ToBytes();
        set => ScriptPubKey = new Script(value);
    }

    /// <inheritdoc />
    public BitcoinScript RedeemBitcoinScript
    {
        get => RedeemScript.ToBytes();
        set => RedeemScript = new Script(value);
    }

    /// <summary>
    /// Gets or sets the transaction ID of the output.
    /// </summary>
    public TxId TxId
    {
        get => TxIdHash.ToBytes();
        set => TxIdHash = new uint256(value);
    }

    /// <inheritdoc />
    public uint Index { get; set; }

    public abstract ScriptType ScriptType { get; }

    protected BaseOutput(LightningMoney amount, Script redeemScript, Script scriptPubKey)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(scriptPubKey);
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        RedeemScript = redeemScript;
        ScriptPubKey = scriptPubKey;
        TxIdHash = uint256.Zero;
    }

    protected BaseOutput(LightningMoney amount, Script redeemScript)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(amount);

        Amount = amount;
        RedeemScript = redeemScript;
        ScriptPubKey = ScriptType switch
        {
            ScriptType.P2WPKH
             or ScriptType.P2WSH
                when redeemScript.ToString().StartsWith("0 ") => redeemScript,
            ScriptType.P2WPKH
             or ScriptType.P2WSH => redeemScript.WitHash.ScriptPubKey,
            ScriptType.P2SH => redeemScript.Hash.ScriptPubKey,
            _ => redeemScript.PaymentScript
        };
        TxIdHash = uint256.Zero;
    }

    /// <summary>
    /// Converts the output to a NBitcoin.TxOut.
    /// </summary>
    /// <returns>TxOut object.</returns>
    public TxOut ToTxOut()
    {
        return new TxOut(Money.Satoshis(Amount.Satoshi), ScriptPubKey);
    }

    public int CompareTo(IOutput? other) =>
        other is BaseOutput baseOutput
            ? TransactionOutputComparer.Instance.Compare(this, baseOutput)
            : 1;
}