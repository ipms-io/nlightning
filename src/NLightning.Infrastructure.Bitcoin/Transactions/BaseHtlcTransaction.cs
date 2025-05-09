using NBitcoin;
using NBitcoin.Crypto;

namespace NLightning.Infrastructure.Bitcoin.Transactions;

using Domain.Money;
using Domain.Protocol.Constants;
using Domain.Protocol.Signers;
using Outputs;

public abstract class BaseHtlcTransaction : BaseTransaction
{
    public HtlcResolutionOutput HtlcResolutionOutput { get; }

    protected BaseHtlcTransaction(bool hasAnchorOutputs, Network network, BaseHtlcOutput htlcOutput, PubKey revocationPubKey,
                                  PubKey localDelayedPubKey, ulong toSelfDelay, ulong amountMilliSats)
        : base(hasAnchorOutputs, network, TransactionConstants.HTLC_TRANSACTION_VERSION,
               hasAnchorOutputs
                   ? SigHash.Single | SigHash.AnyoneCanPay
                   : SigHash.All,
               (htlcOutput.ToCoin(), new Sequence(hasAnchorOutputs ? 1 : 0)))
    {
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

    public void AppendRemoteSignatureAndSign(ILightningSigner signer, ECDSASignature remoteSignature, PubKey remotePubKey)
    {
        AppendRemoteSignatureToTransaction(signer, new TransactionSignature(remoteSignature), remotePubKey);
        SignTransactionWithExistingKeys(signer);
    }
}