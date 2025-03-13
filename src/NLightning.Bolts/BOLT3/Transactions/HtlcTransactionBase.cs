using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

using Constants;
using Outputs;

public abstract class HtlcTransactionBase : Transaction
{
    public HtlcOutput HtlcOutput { get; }

    protected HtlcTransactionBase(PubKey revocationPubKey, PubKey localDelayedPubKey, ulong toSelfDelay, ulong amountMilliSats, ulong feeSats)
    {
        Version = TransactionConstants.HTLC_TRANSACTION_VERSION;

        var amountSats = (ulong)Math.Round(amountMilliSats / 1_000M, MidpointRounding.ToZero) - feeSats;

        HtlcOutput = new HtlcOutput(revocationPubKey, localDelayedPubKey, toSelfDelay, amountSats);

        Outputs.Add(HtlcOutput.ToTxOut());
    }
}