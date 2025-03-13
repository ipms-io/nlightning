using NBitcoin;
using NLightning.Bolts.BOLT3.Constants;

namespace NLightning.Bolts.BOLT3.Transactions;

using Calculators;
using Common.Managers;
using Outputs;

/// <summary>
/// Represents a funding transaction.
/// </summary>
public class FundingTransaction
{
    private readonly Transaction _transaction;
    private readonly List<Coin> _coins;
    private readonly ulong _fundingAmountSats;

    public FundingOutput FundingOutput { get; }
    public ChangeOutput ChangeOutput { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FundingTransaction"/> class.
    /// </summary>
    /// <param name="pubkey1">The first public key in compressed format.</param>
    /// <param name="pubkey2">The second public key in compressed format.</param>
    /// <param name="amountSats">The amount of the output in satoshis.</param>
    /// <param name="changeScript">The script for the change output.</param>
    /// <param name="coins">The coins to be used in the transaction.</param>
    internal FundingTransaction(PubKey pubkey1, PubKey pubkey2, ulong amountSats, Script changeScript,
                                params Coin[] coins)
    {
        ArgumentNullException.ThrowIfNull(pubkey1);
        ArgumentNullException.ThrowIfNull(pubkey2);

        if (pubkey1 == pubkey2)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats == 0)
            throw new ArgumentException("Funding amount must be greater than zero.");

        _fundingAmountSats = amountSats;
        _coins = coins.ToList();
        _transaction = Transaction.Create(ConfigManager.Instance.Network);
        _transaction.Version = TransactionConstants.FUNDING_TRANSACTION_VERSION;
        _transaction.Inputs.AddRange(_coins.Select(c => new TxIn(c.Outpoint)));

        // Create the funding and change output
        FundingOutput = new FundingOutput(pubkey1, pubkey2, amountSats);
        ChangeOutput = new ChangeOutput(changeScript, 0);
    }

    internal Transaction SignAndFinalizeTransaction(FeeCalculator feeCalculator, params Key[] keys)
    {
        // Get total input amount
        var totalInputAmount = _coins.Sum(c => c.Amount.Satoshi);

        // Check if output amount is greater than input amount
        if (_fundingAmountSats >= (ulong)totalInputAmount)
            throw new InvalidOperationException("Output amount cannot exceed input amount + fees.");

        // Calculate script length sum for existing outputs and inputs
        var changeScriptLength = ChangeOutput.ScriptPubKey.ToBytes().Length;
        var changeMinValue = (changeScriptLength + FeeCalculator.SEGWIT_OUTPUT_BASE_SIZE)
                             * (long)feeCalculator.GetCurrentEstimatedFeePerKw()
                             / 1000;

        // Sign all inputs
        _transaction.Sign(keys.Select(k => new BitcoinSecret(k, ConfigManager.Instance.Network)),
                          _coins.Select(c => c as ICoin));

        // Calculate signature/witness size
        var witnessSize = 0;
        var inputSize = 0;
        foreach (var input in _transaction.Inputs)
        {
            var coin = _coins.SingleOrDefault(c => c.Outpoint == input.PrevOut);
            switch (coin)
            {
                case null:
                    throw new NullReferenceException("Can't find coin for a input.");
                case ScriptCoin:
                    witnessSize += input.WitScript.ToBytes().Length;
                    inputSize += FeeCalculator.SEGWIT_INPUT_SIZE;
                    break;
                default:
                    inputSize += input.ToBytes().Length;
                    break;
            }
        }

        // Calculate fee
        var fee = feeCalculator.CalculateFundingTransactionFee(inputSize, 1, changeScriptLength, witnessSize);

        // Get change amount and check if it's dust
        var changeAmount = totalInputAmount - (long)_fundingAmountSats - (long)fee;
        if (changeAmount < 0 || changeAmount < changeMinValue)
        {
            // TODO: Log warning about dust output
        }
        else
        {
            // Update change output amount
            ChangeOutput.AmountSats = (ulong)changeAmount;

            if (FundingOutput.CompareTo(ChangeOutput) < 0)
            {
                _transaction.Outputs.Add(FundingOutput.ToTxOut());
                _transaction.Outputs.Add(ChangeOutput.ToTxOut());
            }
            else
            {
                _transaction.Outputs.Add(ChangeOutput.ToTxOut());
                _transaction.Outputs.Add(FundingOutput.ToTxOut());
            }
        }

        // Check if the transaction is valid
        if (_transaction.Check() != TransactionCheckResult.Success)
            throw new InvalidOperationException("Transaction is not valid.");

        return _transaction;
    }
}