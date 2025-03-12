using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

using Common.Managers;

public class HtlcSuccessTransaction : HtlcTransactionBase
{
    public HtlcSuccessTransaction(OutPoint outPoint,
                                  PubKey revocationPubKey,
                                  PubKey localDelayedPubKey,
                                  ReadOnlySpan<byte> remoteHtlcSignature,
                                  ReadOnlySpan<byte> localHtlcSignature,
                                  uint cltvEpiry,
                                  ulong toSelfDelay,
                                  ulong amountMilliSats,
                                  ulong feesSats)
        : base(revocationPubKey, localDelayedPubKey, toSelfDelay, amountMilliSats, feesSats)
    {
        LockTime = new LockTime(cltvEpiry);
        Inputs.Add(new TxIn
        {
            PrevOut = outPoint,
            Sequence = new Sequence(ConfigManager.Instance.IsOptionAnchorOutput ? 1 : 0)
        });
    }
}