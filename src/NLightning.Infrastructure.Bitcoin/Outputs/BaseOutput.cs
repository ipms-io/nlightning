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
    internal uint256 TxIdHash;
    internal Script RedeemScript;
    internal Money NBitcoinAmount;

    /// <inheritdoc />
    public LightningMoney Amount
    {
        get => LightningMoney.Satoshis(NBitcoinAmount.Satoshi);
        set => Money.Satoshis(value.Satoshi);
    }

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
    public TxId TransactionId
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

        NBitcoinAmount = Money.Satoshis(amount.Satoshi);
        RedeemScript = redeemScript;
        ScriptPubKey = scriptPubKey;
        TxIdHash = uint256.Zero;
    }

    protected BaseOutput(LightningMoney amount, Script redeemScript)
    {
        ArgumentNullException.ThrowIfNull(redeemScript);
        ArgumentNullException.ThrowIfNull(amount);

        NBitcoinAmount = Money.Satoshis(amount.Satoshi);
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

    public ScriptCoin ToCoin()
    {
        if (TxIdHash is null || TxIdHash == uint256.Zero || TxIdHash == uint256.One)
            throw new InvalidOperationException("Transaction ID is not set. Sign the transaction first.");

        if (Amount.IsZero)
            throw new InvalidOperationException("You can't spend a zero amount output.");

        return new ScriptCoin(TxIdHash, Index, Money.Satoshis(Amount.Satoshi), ScriptPubKey, RedeemScript);
    }

    public int CompareTo(IOutput? other) =>
        other is BaseOutput baseOutput
            ? TransactionOutputComparer.Instance.Compare(this, baseOutput)
            : 1;
}