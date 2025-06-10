namespace NLightning.Infrastructure.Bitcoin.Comparers;

using Outputs;

public class TransactionOutputComparer : IComparer<BaseOutput>
{
    public static TransactionOutputComparer Instance { get; } = new();

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
        var valueComparison = x.Amount.Satoshi.CompareTo(y.Amount.Satoshi);
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

        // For HTLC outputs, compare by CLTV expiry
        if (x is OfferedHtlcOutput or ReceivedHtlcOutput &&
            y is OfferedHtlcOutput or ReceivedHtlcOutput)
        {
            ulong xExpiry = x switch
            {
                OfferedHtlcOutput offered => offered.CltvExpiry,
                ReceivedHtlcOutput received => received.CltvExpiry,
                _ => 0
            };

            ulong yExpiry = y switch
            {
                OfferedHtlcOutput offered => offered.CltvExpiry,
                ReceivedHtlcOutput received => received.CltvExpiry,
                _ => 0
            };

            if (xExpiry != yExpiry)
            {
                return xExpiry.CompareTo(yExpiry);
            }
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

        // Compare by length if scripts are identical up to the length of the shorter one
        return script1.Length.CompareTo(script2.Length);
    }
}