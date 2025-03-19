namespace NLightning.Bolts.BOLT3.Comparers;

using Outputs;

public class TransactionOutputComparer : IComparer<BaseOutput>
{
    public static TransactionOutputComparer Instance { get; } = new TransactionOutputComparer();

    public int Compare(BaseOutput? x, BaseOutput? y)
    {
        switch (x, y)
        {
            // Deal with nulls
            case (null, null):
                return 0;
            case (null, not null):
                return -1;
            case (not null, null):
                return 1;
        }

        // Compare by value (satoshis)
        var valueComparison = x.Amount.CompareTo(y.Amount);
        if (valueComparison != 0)
        {
            return valueComparison;
        }

        // Compare by scriptPubKey lexicographically
        var scriptComparison = CompareScriptPubKey(x.ScriptPubKey.ToBytes(), y.ScriptPubKey.ToBytes());
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
        if (x is OfferedHtlcOutput xHtlc && y is OfferedHtlcOutput yHtlc)
        {
            return xHtlc.CltvExpiry != yHtlc.CltvExpiry ? xHtlc.CltvExpiry.CompareTo(yHtlc.CltvExpiry) : 0;
        }

        return 0;
    }

    private static int CompareScriptPubKey(ReadOnlySpan<byte> script1, ReadOnlySpan<byte> script2)
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