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
    private readonly LightningMoney _fundingAmountSats;

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
    internal FundingTransaction(PubKey pubkey1, PubKey pubkey2, LightningMoney amountSats, Script changeScript,
                                params Coin[] coins) : base(TransactionConstants.FUNDING_TRANSACTION_VERSION, coins)
    {
        ArgumentNullException.ThrowIfNull(pubkey1);
        ArgumentNullException.ThrowIfNull(pubkey2);

        if (pubkey1 == pubkey2)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats.IsZero)
            throw new ArgumentException("Funding amount must be greater than zero.");

        _fundingAmountSats = amountSats;

        // Create the funding and change output
        FundingOutput = new FundingOutput(pubkey1, pubkey2, amountSats);
        ChangeOutput = new ChangeOutput(changeScript);

        AddOutput(FundingOutput);
        AddOutput(ChangeOutput);
    }
    internal FundingTransaction(PubKey pubkey1, PubKey pubkey2, LightningMoney amountSats, Script redeemScript,
                                Script changeScript, params Coin[] coins)
        : base(TransactionConstants.FUNDING_TRANSACTION_VERSION, coins)
    {
        ArgumentNullException.ThrowIfNull(pubkey1);
        ArgumentNullException.ThrowIfNull(pubkey2);

        if (pubkey1 == pubkey2)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats.IsZero)
            throw new ArgumentException("Funding amount must be greater than zero.");

        _fundingAmountSats = amountSats;

        // Create the funding and change output
        FundingOutput = new FundingOutput(pubkey1, pubkey2, amountSats);
        ChangeOutput = new ChangeOutput(redeemScript, changeScript);

        AddOutput(FundingOutput);
        AddOutput(ChangeOutput);
    }

    internal void SignTransaction(FeeCalculator feeCalculator, params BitcoinSecret[] secrets)
    {
        SignAndFinalizeTransaction(feeCalculator, secrets);

        // Remove the old change output (zero value)
        RemoveOutput(ChangeOutput);

        // Check if change is needed
        var changeAmount = TotalInputAmount - TotalOutputAmount - CalculatedFee;
        var changeIndex = 0U;
        var hasChange = changeAmount >= ConfigManager.Instance.DustLimitAmountSats;
        if (hasChange)
        {
            // Add the new one
            ChangeOutput.AmountMilliSats = changeAmount;
            changeIndex = (uint)AddOutput(ChangeOutput);
        }

        SignAndFinalizeTransaction(feeCalculator, secrets);

        // Set funding output fields
        FundingOutput.TxId = TxId;
        FundingOutput.Index = changeIndex == 0 ? 1U : 0;

        if (hasChange)
        {
            // Set change output fields
            ChangeOutput.TxId = TxId;
            ChangeOutput.Index = changeIndex;
        }
    }

    public Transaction GetSignedTransaction()
    {
        if (Finalized)
        {
            return FinalizedTransaction;
        }

        throw new InvalidOperationException("You have to sign and finalize the transaction first.");
    }
}