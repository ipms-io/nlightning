using NLightning.Bolts.BOLT3.Transactions;

namespace NLightning.Bolts.BOLT3.Comparers;

public class TransactionOutputComparer : IComparer<TransactionOutput>
{
    public int Compare(TransactionOutput? x, TransactionOutput? y)
    {
        // Deal with nulls
        if (x == null && y == null) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // Compare by value (satoshis)
        var valueComparison = x.Value.CompareTo(y.Value);
        if (valueComparison != 0)
        {
            return valueComparison;
        }

        // Compare by scriptPubKey lexicographically
        var scriptComparison = CompareScriptPubKey(x.ScriptPubKey, y.ScriptPubKey);
        if (scriptComparison != 0)
        {
            return scriptComparison;
        }

        // Compare by length if scripts are identical up to the length of the shorter one
        if (x.ScriptPubKey.Length != y.ScriptPubKey.Length)
        {
            return x.ScriptPubKey.Length.CompareTo(y.ScriptPubKey.Length);
        }

        // For HTLC outputs, compare by CLTV expiry
        return x.CltvExpiry != y.CltvExpiry ? x.CltvExpiry.CompareTo(y.CltvExpiry) : 0;
    }

    private static int CompareScriptPubKey(byte[] script1, byte[] script2)
    {
        var length = Math.Min(script1.Length, script2.Length);
        for (var i = 0; i < length; i++)
        {
            if (script1[i] != script2[i])
            {
                return script1[i].CompareTo(script2[i]);
            }
        }

        return script1.Length.CompareTo(script2.Length);
    }
}