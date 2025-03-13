using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

using Calculators;
using Common.Managers;
using Constants;
using Outputs;

/// <summary>
/// Represents a funding transaction.
/// </summary>
public class FundingTransaction : BaseTransaction
{
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
                                params Coin[] coins) : base(TransactionConstants.FUNDING_TRANSACTION_VERSION, coins)
    {
        ArgumentNullException.ThrowIfNull(pubkey1);
        ArgumentNullException.ThrowIfNull(pubkey2);

        if (pubkey1 == pubkey2)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats == 0)
            throw new ArgumentException("Funding amount must be greater than zero.");

        _fundingAmountSats = amountSats;

        // Create the funding and change output
        FundingOutput = new FundingOutput(pubkey1, pubkey2, amountSats);
        ChangeOutput = new ChangeOutput(changeScript);

        AddOutput(FundingOutput);
        AddOutput(ChangeOutput);
    }

    internal Transaction GetSignedTransaction(FeeCalculator feeCalculator, params BitcoinSecret[] secrets)
    {
        SignAndFinalizeTransaction(feeCalculator, secrets);

        // Remove the old change output (zero value)
        RemoveOutput(ChangeOutput);

        // Check if change is needed
        var changeAmount = TotalInputAmount - TotalOutputAmount - CalculatedFee;
        if (changeAmount < (long)ConfigManager.Instance.DustLimitAmountSats)
        {
            return FinalizedTransaction;
        }

        // Add the new one
        ChangeOutput.AmountSats = (ulong)changeAmount;
        AddOutput(ChangeOutput);

        // Get the newest transaction
        return FinalizedTransaction;

        /*
        // Get total input amount
        var totalInputAmount = COINS.Sum(c => c.Amount.Satoshi);

        // Check if output amount is greater than input amount
        if (_fundingAmountSats >= (ulong)totalInputAmount)
            throw new InvalidOperationException("Output amount cannot exceed input amount + fees.");

        // Sign all inputs
        var signedTx = SignTransaction(secrets);

        // Calculate signature/witness size
        var witnessSize = 0;
        var inputSize = 0;
        foreach (var input in signedTx.Inputs)
        {
            var coin = COINS.SingleOrDefault(c => c.Outpoint == input.PrevOut);
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
        var fee = feeCalculator.CalculateFundingTransactionFee(inputSize, 1, ChangeOutput.ScriptPubKey.ToBytes().Length, witnessSize);

        // Get change amount and check if it's dust
        var changeAmount = totalInputAmount - (long)_fundingAmountSats - (long)fee;
        if (changeAmount < 0 || changeAmount < (long)ConfigManager.Instance.DustLimitAmountSats)
        {
            // TODO: Log warning about dust output
        }
        else
        {
            // Update change output amount
            ChangeOutput.AmountSats = (ulong)changeAmount;

            if (FundingOutput.CompareTo(ChangeOutput) < 0)
            {
                TRANSACTION.Outputs.Add(FundingOutput.ToTxOut());
                TRANSACTION.Outputs.Add(ChangeOutput.ToTxOut());
            }
            else
            {
                TRANSACTION.Outputs.Add(ChangeOutput.ToTxOut());
                TRANSACTION.Outputs.Add(FundingOutput.ToTxOut());
            }
        }

        // Check if the transaction is valid
        if (TRANSACTION.Check() != TransactionCheckResult.Success)
            throw new InvalidOperationException("Transaction is not valid.");

        return TRANSACTION;
        */
    }
}