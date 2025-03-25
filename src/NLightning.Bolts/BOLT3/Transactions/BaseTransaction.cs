using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

using Common.Interfaces;
using Common.Managers;
using Comparers;
using Constants;
using Outputs;

public abstract class BaseTransaction
{
    private Transaction _transaction;
    private readonly TransactionBuilder _builder = ((NBitcoin.Network)ConfigManager.Instance.Network).CreateTransactionBuilder();

    private readonly SigHash _sigHash = ConfigManager.Instance.IsOptionAnchorOutput
        ? SigHash.Single | SigHash.AnyoneCanPay
        : SigHash.All;
    private readonly List<(Coin, Sequence)> _coins = [];

    protected readonly List<BaseOutput> OUTPUTS = [];

    public uint256 TxId { get; private set; } = uint256.Zero;

    protected LightningMoney CalculatedFee { get; } = LightningMoney.Zero;
    protected bool Finalized { get; private set; }

    protected BaseTransaction(uint version, params Coin[] coins)
    {
        _coins = coins.Select(c => (c, Sequence.Final)).ToList();

        _transaction = Transaction.Create(ConfigManager.Instance.Network);
        _transaction.Version = version;
        _transaction.Inputs.AddRange(_coins.Select(c => new TxIn(c.Item1.Outpoint)));
        _builder.SetSigningOptions(_sigHash, false);
        _builder.DustPrevention = false;
        _builder.SetVersion(version);
    }

    protected BaseTransaction(uint version, params (Coin, Sequence)[] coins)
    {
        _transaction = Transaction.Create(ConfigManager.Instance.Network);
        _transaction.Version = version;
        _builder.SetSigningOptions(_sigHash, false);
        _builder.DustPrevention = false;
        _builder.SetVersion(version);

        _coins.AddRange(coins);
        foreach (var (coin, sequence) in _coins)
        {
            _transaction.Inputs.Add(coin.Outpoint, null, null, sequence);
        }
    }

    protected Transaction FinalizedTransaction => Finalized ? _transaction : throw new Exception("Transaction not finalized.");

    protected void SetLockTime(LockTime lockTime)
    {
        ArgumentNullException.ThrowIfNull(lockTime);

        _transaction.LockTime = lockTime;
    }

    protected void SignTransaction(params BitcoinSecret[] secrets)
    {
        ArgumentNullException.ThrowIfNull(secrets);

        // Check if output amount is greater than input amount
        if (!CheckTransactionAmounts())
            throw new InvalidOperationException("Output amount cannot exceed input amount.");

        // Sign all inputs
        ArgumentNullException.ThrowIfNull(secrets);

        if (Finalized)
        {
            // Remove signature from inputs
            _transaction.Inputs.Clear();
            foreach (var (coin, sequence) in _coins)
            {
                _transaction.Inputs.Add(coin.Outpoint, null, null, sequence);
            }
        }
        else
        {
            // Add our keys
            _builder.AddKeys(secrets);
            _builder.AddCoins(_coins.Select(c => c.Item1));
        }

        _transaction = _builder.SignTransactionInPlace(_transaction);

        TxId = _transaction.GetHash();
        Finalized = true;
    }

    protected void CalculateAndCheckFees(IFeeService feeService)
    {
        // Calculate transaction fee
        CalculateTransactionFee(feeService);

        // Check if output amount + fees is greater than input amount
        if (!CheckTransactionAmounts(CalculatedFee))
            throw new InvalidOperationException("Output amount cannot exceed input amount.");
    }

    protected void AppendRemoteSignatureToTransaction(ITransactionSignature remoteSignature, PubKey remotePubKey)
    {
        _builder.AddKnownSignature(remotePubKey, remoteSignature, _transaction.Inputs[0].PrevOut);
    }

    protected void SignTransactionWithExistingKeys()
    {
        _transaction = _builder.SignTransactionInPlace(_transaction);

        TxId = _transaction.GetHash();
        Finalized = true;
    }

    protected LightningMoney TotalInputAmount => _coins.Sum(c => (LightningMoney)c.Item1.Amount);

    protected LightningMoney TotalOutputAmount => OUTPUTS.Sum(o => o.Amount);

    protected bool CheckTransactionAmounts(LightningMoney? fees = null)
    {
        // Check if output amount is greater than input amount
        return TotalOutputAmount + (fees ?? LightningMoney.Zero) <= TotalInputAmount;
    }

    protected int CalculateOutputWeight()
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
                case OfferedHtlcOutput:
                case ReceivedHtlcOutput:
                    outputWeight += WeightConstants.HTLC_OUTPUT_WEIGHT;
                    break;
            }
        }

        return outputWeight;
    }

    protected int CalculateInputWeight()
    {
        var inputWeight = 0;
        var mustAddWitnessHeader = false;

        foreach (var (coin, _) in _coins)
        {
            var input = _transaction.Inputs.SingleOrDefault(i => i.PrevOut == coin.Outpoint) ?? throw new NullReferenceException("Input not found in transaction.");

            if (input.WitScript.PushCount > 0)
            {
                mustAddWitnessHeader = true;
            }

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
                inputWeight += Math.Max(WeightConstants.MULTISIG_WITNESS_WEIGHT, input.WitScript.ToBytes().Length);
            }
            else
            {
                inputWeight += 4 * Math.Max(WeightConstants.P2UNKOWN_S_INTPUT_WEIGHT, input.ToBytes().Length);
                inputWeight += input.WitScript.ToBytes().Length;
            }
        }

        if (mustAddWitnessHeader)
        {
            inputWeight += WeightConstants.WITNESS_HEADER;
        }

        return inputWeight;
    }

    protected void CalculateTransactionFee(IFeeService feeService)
    {
        var outputWeight = CalculateOutputWeight();
        var inputWeight = CalculateInputWeight();

        CalculatedFee.Satoshi = (outputWeight + inputWeight) * feeService.GetCachedFeeRatePerKw().Satoshi / 1000L;
    }

    #region Input Management
    protected void AddCoin(Coin coin, Sequence sequence)
    {
        ArgumentNullException.ThrowIfNull(coin);

        _transaction.Inputs.Add(coin.Outpoint, null, null, sequence);
    }
    protected void AddCoin(Coin coin)
    {
        ArgumentNullException.ThrowIfNull(coin);

        _coins.Add((coin, Sequence.Final));
        _transaction.Inputs.Add(coin.Outpoint, null, null, Sequence.Final);
    }
    #endregion

    #region Output Management
    protected int AddOutput(BaseOutput baseOutput)
    {
        ArgumentNullException.ThrowIfNull(baseOutput);

        OUTPUTS.Add(baseOutput);

        // OrderOutputs();

        return OUTPUTS.IndexOf(baseOutput);
    }

    protected void AddOutputRange(IEnumerable<BaseOutput> outputs)
    {
        ArgumentNullException.ThrowIfNull(outputs);

        var outputBases = outputs as BaseOutput[] ?? outputs.ToArray();
        if (outputBases.Length == 0)
            return;

        foreach (var output in outputBases)
        {
            ArgumentNullException.ThrowIfNull(output);
            OUTPUTS.Add(output);
        }

        // OrderOutputs();
    }

    protected void ClearOutputsFromTransaction()
    {
        _transaction.Outputs.Clear();
    }

    protected void RemoveOutput(BaseOutput baseOutput)
    {
        ArgumentNullException.ThrowIfNull(baseOutput);

        OUTPUTS.Remove(baseOutput);
        // OrderOutputs();
    }

    protected void AddOrderedOutputsToTransaction()
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