using NBitcoin;
using NLightning.Common.Interfaces;

namespace NLightning.Bolts.BOLT3.Transactions;

using Outputs;

public class HtlcSuccessTransaction : BaseHtlcTransaction
{
    public byte[] PaymentPreimage { get; }

    public HtlcSuccessTransaction(IFeeService feeService, BaseHtlcOutput output, PubKey revocationPubKey,
                                  PubKey localDelayedPubKey, ulong toSelfDelay, ulong amountMilliSats,
                                  byte[] paymentPreimage)
        : base(feeService, output, revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats)
    {
        SetLockTime(0);
        PaymentPreimage = paymentPreimage;
    }

    protected new void SignTransaction(params BitcoinSecret[] secrets)
    {
        var witness = new WitScript(
            Op.GetPushOp(0), // OP_0
            Op.GetPushOp(0), // Remote signature
            Op.GetPushOp(0), // Local signature
            Op.GetPushOp(PaymentPreimage) // Payment pre-image for HTLC-success
        );

        base.SignTransaction(secrets);
    }
}