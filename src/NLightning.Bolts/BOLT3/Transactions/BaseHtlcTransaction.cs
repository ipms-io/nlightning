using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Bolts.BOLT3.Transactions;

using Common.Interfaces;
using Common.Managers;
using Constants;
using Outputs;

public abstract class BaseHtlcTransaction : BaseTransaction
{
    private readonly IFeeService _feeService;
    public HtlcResolutionOutput HtlcResolutionOutput { get; }

    protected BaseHtlcTransaction(IFeeService feeService, BaseHtlcOutput htlcOutput, PubKey revocationPubKey,
                                  PubKey localDelayedPubKey, ulong toSelfDelay, ulong amountMilliSats)
        : base(TransactionConstants.HTLC_TRANSACTION_VERSION,
               ConfigManager.Instance.IsOptionAnchorOutput
                   ? SigHash.Single | SigHash.AnyoneCanPay
                   : SigHash.All,
               (htlcOutput.ToCoin(), new Sequence(ConfigManager.Instance.IsOptionAnchorOutput ? 1 : 0)))
    {
        _feeService = feeService;
        HtlcResolutionOutput = new HtlcResolutionOutput(revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats);
    }

    internal override void ConstructTransaction(LightningMoney currentFeePerKw)
    {
        // Calculate transaction fee
        CalculateTransactionFee(currentFeePerKw);

        HtlcResolutionOutput.Amount -= CalculatedFee;

        AddOrderedOutputsToTransaction();

        HtlcResolutionOutput.TxId = TxId;
        HtlcResolutionOutput.Index = 0;
    }

    public void AppendRemoteSignatureAndSign(ECDSASignature remoteSignature, PubKey remotePubKey)
    {
        AppendRemoteSignatureToTransaction(new TransactionSignature(remoteSignature), remotePubKey);
        SignTransactionWithExistingKeys();
    }
}