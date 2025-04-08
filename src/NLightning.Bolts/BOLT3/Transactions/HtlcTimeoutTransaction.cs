using NBitcoin;
using NLightning.Bolts.BOLT3.Outputs;
using NLightning.Common.Interfaces;

namespace NLightning.Bolts.BOLT3.Transactions;
public class HtlcTimeoutTransaction : BaseHtlcTransaction
{
    public HtlcTimeoutTransaction(IFeeService feeService, BaseHtlcOutput output, PubKey revocationPubKey,
                                  PubKey localDelayedPubKey, uint cltvEpiry, ulong toSelfDelay, ulong amountMilliSats)
        : base(feeService, output, revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats)
    {
        SetLockTime(cltvEpiry);
    }

    protected new void SignTransaction(params BitcoinSecret[] secrets)
    {
        var witness = new WitScript(
            Op.GetPushOp(0), // OP_0
            Op.GetPushOp(0), // Remote signature
            Op.GetPushOp(0), // Local signature
            Op.GetPushOp([]) // Payment pre-image for HTLC-success
        );

        base.SignTransaction(secrets);
    }
}