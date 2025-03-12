using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

public class ClosingTransaction : Transaction
{
    public ulong CloserOutputAmountSatoshis { get; }
    public ulong CloseeOutputAmountSatoshis { get; }
    
    public ClosingTransaction(OutPoint outPoint, ulong closerAmountSatoshis, ulong closeeAmountSatoshis, Script closerScriptPubKey, Script closeeScriptPubKey, ulong feeSatoshis)
    {
        Version = 2;
        LockTime = 0; // TODO: Find out correct lockTime
        
        Inputs.Add(new TxIn
        {
            PrevOut = outPoint,
            Sequence = 0xFFFFFFFD
        });
        
        CloserOutputAmountSatoshis = CalculateOutputAmount(closerAmountSatoshis, closerScriptPubKey, feeSatoshis);
        Outputs.Add(new TxOut((Money)CloserOutputAmountSatoshis, closerScriptPubKey));

        if (closeeAmountSatoshis == 0)
        {
            return;
        }

        CloseeOutputAmountSatoshis = CalculateOutputAmount(closeeAmountSatoshis, closeeScriptPubKey, 0);
        if (CloseeOutputAmountSatoshis > 0)
        {
            Outputs.Add(new TxOut((Money)CloseeOutputAmountSatoshis, closeeScriptPubKey));
        }
    }

    private static ulong CalculateOutputAmount(ulong amountSatoshis, Script scriptPubKey, ulong feeSatoshis)
    {
        if (scriptPubKey.ToBytes()[0] == (byte)OpcodeType.OP_RETURN)
        {
            return 0;
        }

        var finalAmount = amountSatoshis - feeSatoshis;
        return finalAmount > 0 ? finalAmount : 0;
    }
}