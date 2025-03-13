using NBitcoin;
using NLightning.Bolts.BOLT3.Calculators;
using NLightning.Bolts.BOLT3.Constants;

namespace NLightning.Bolts.BOLT3.Transactions;

using Common.Managers;
using Comparers;
using Outputs;

public abstract class BaseTransaction
{
    private readonly Transaction _transaction;

    protected readonly List<Coin> COINS;
    protected readonly List<OutputBase> OUTPUTS = [];

    protected long CalculatedFee { get; private set; }
    protected bool Finalized { get; private set; }

    protected BaseTransaction(uint version, params Coin[] coins)
    {
        COINS = coins.ToList();
        _transaction = Transaction.Create(ConfigManager.Instance.Network);

        _transaction.Version = version;
        _transaction.Inputs.AddRange(COINS.Select(c => new TxIn(c.Outpoint)));
    }

    protected Transaction FinalizedTransaction => Finalized ? _transaction : throw new Exception("Transaction not finalized.");

    protected void SignAndFinalizeTransaction(FeeCalculator feeCalculator, params BitcoinSecret[] secrets)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        // Check if output amount is greater than input amount
        if (!CheckTransactionAmounts())
            throw new InvalidOperationException("Output amount cannot exceed input amount.");

        // Sign all inputs
        SignTransaction(secrets);

        // Calculate transaction fee
        CalculateTransactionFee(feeCalculator);

        // Check if output amount + fees is greater than input amount
        if (!CheckTransactionAmounts(CalculatedFee))
            throw new InvalidOperationException("Output amount cannot exceed input amount.");

        Finalized = true;
    }

    protected long TotalInputAmount => COINS.Sum(c => c.Amount.Satoshi);

    protected long TotalOutputAmount => OUTPUTS.Sum(o => (long)o.AmountSats);

    private void SignTransaction(params BitcoinSecret[] secrets)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        // Sign all inputs
        _transaction.Sign(secrets, COINS);
    }

    private bool CheckTransactionAmounts(long fees = 0)
    {
        // Check if output amount is greater than input amount
        return TotalOutputAmount + fees < TotalInputAmount;
    }

    private int CalculateOutputWeight()
    {
        var outputWeight = WeightConstants.TRANSACTION_BASE_WEIGHT;

        foreach (var output in OUTPUTS)
        {
            switch (output)
            {
                case FundingOutput:
                    outputWeight += WeightConstants.P2WSH_OUTPUT_WEIGHT;
                    break;
                case ChangeOutput changeOutput when changeOutput.ScriptPubKey.IsScriptType(ScriptType.P2PKH):
                    outputWeight += WeightConstants.P2PKH_OUTPUT_WEIGHT;
                    break;
                case ChangeOutput changeOutput when changeOutput.ScriptPubKey.IsScriptType(ScriptType.P2SH):
                    outputWeight += WeightConstants.P2SH_OUTPUT_WEIGHT;
                    break;
                case ChangeOutput changeOutput when changeOutput.ScriptPubKey.IsScriptType(ScriptType.P2WPKH):
                    outputWeight += WeightConstants.P2WPKH_OUTPUT_WEIGHT;
                    break;
                case ChangeOutput changeOutput when changeOutput.ScriptPubKey.IsScriptType(ScriptType.P2WSH):
                    outputWeight += WeightConstants.P2WSH_OUTPUT_WEIGHT;
                    break;
                case ChangeOutput changeOutput:
                    outputWeight += changeOutput.ScriptPubKey.Length;
                    break;
                case ToLocalOutput:
                case ToRemoteOutput when ConfigManager.Instance.IsOptionAnchorOutput:
                    outputWeight += WeightConstants.P2WSH_OUTPUT_WEIGHT;
                    break;
                case ToRemoteOutput:
                    outputWeight += WeightConstants.P2WPKH_OUTPUT_WEIGHT;
                    break;
                case ToAnchorOutput:
                    outputWeight += WeightConstants.ANCHOR_OUTPUT_WEIGHT;
                    break;
                case HtlcOutput:
                    outputWeight += WeightConstants.HTLC_OUTPUT_WEIGHT;
                    break;
            }
        }

        return outputWeight;
    }

    private int CalculateInputWeight()
    {
        var inputWeight = 0;

        foreach (var coin in COINS)
        {
            var input = _transaction.Inputs.SingleOrDefault(i => i.PrevOut == coin.Outpoint) ?? throw new NullReferenceException("Input not found in transaction.");
            if (coin.ScriptPubKey.IsScriptType(ScriptType.P2PKH))
            {
                inputWeight += 4 * Math.Max(WeightConstants.P2PKH_INTPUT_WEIGHT, input.ToBytes().Length);
            }
            else if (coin.ScriptPubKey.IsScriptType(ScriptType.P2SH))
            {
                inputWeight += 4 * Math.Max(WeightConstants.P2SH_INTPUT_WEIGHT, input.ToBytes().Length);
                inputWeight += input.WitScript.ToBytes().Length;
            }
            else if (coin.ScriptPubKey.IsScriptType(ScriptType.P2WPKH))
            {
                inputWeight += 4 * Math.Max(WeightConstants.P2WPKH_INTPUT_WEIGHT, input.ToBytes().Length);
                inputWeight += input.WitScript.ToBytes().Length;
            }
            else if (coin.ScriptPubKey.IsScriptType(ScriptType.P2WSH))
            {
                inputWeight += 4 * Math.Max(WeightConstants.P2WSH_INTPUT_WEIGHT, input.ToBytes().Length);
                inputWeight += input.WitScript.ToBytes().Length;
            }
            else
            {
                inputWeight += 4 * Math.Max(WeightConstants.P2UNKOWN_S_INTPUT_WEIGHT, input.ToBytes().Length);
                inputWeight += input.WitScript.ToBytes().Length;
            }
        }

        return inputWeight;
    }

    private void CalculateTransactionFee(FeeCalculator feeCalculator)
    {
        var outputWeight = CalculateOutputWeight();
        var inputWeight = CalculateInputWeight();

        CalculatedFee = (outputWeight + inputWeight) * (long)feeCalculator.GetCurrentEstimatedFeePerKw() / 1000;
    }

    protected void AddCoin(Coin coin)
    {
        ArgumentNullException.ThrowIfNull(coin);

        COINS.Add(coin);
        _transaction.Inputs.Add(new TxIn(coin.Outpoint));
    }

    #region Output Management
    protected void AddOutput(OutputBase output)
    {
        ArgumentNullException.ThrowIfNull(output);

        OUTPUTS.Add(output);
        OrderOutputs();
    }

    protected void AddOutputRange(IEnumerable<OutputBase> outputs)
    {
        ArgumentNullException.ThrowIfNull(outputs);

        var outputBases = outputs as OutputBase[] ?? outputs.ToArray();
        if (outputBases.Length == 0)
            return;

        foreach (var output in outputBases)
        {
            ArgumentNullException.ThrowIfNull(output);
            OUTPUTS.Add(output);
        }

        OrderOutputs();
    }

    protected void RemoveOutput(OutputBase output)
    {
        ArgumentNullException.ThrowIfNull(output);

        OUTPUTS.Remove(output);
        OrderOutputs();
    }

    private void OrderOutputs()
    {
        // Clear TxOuts
        _transaction.Outputs.Clear();

        switch (OUTPUTS.Count)
        {
            case 0:
                return;
            case 1:
                _transaction.Outputs.Add(OUTPUTS[0].ToTxOut());
                break;
            default:
                // Add ordered outputs
                _transaction.Outputs.AddRange(
                    OUTPUTS.OrderBy(o => o, TransactionOutputComparer.Instance).Select(o => o.ToTxOut())
                );
                break;
        }
    }
    #endregion
}