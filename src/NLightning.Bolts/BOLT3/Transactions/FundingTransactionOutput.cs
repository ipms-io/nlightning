using NBitcoin;

namespace NLightning.Bolts.BOLT3.Transactions;

/// <summary>
/// Represents a funding transaction output.
/// </summary>
public class FundingTransactionOutput
{
    public byte[] FundingScriptPubKey { get; }
    public ulong Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FundingTransactionOutput"/> class.
    /// </summary>
    /// <param name="pubkey1">The first public key in compressed format.</param>
    /// <param name="pubkey2">The second public key in compressed format.</param>
    /// <param name="value">The value of the output in satoshis.</param>
    public FundingTransactionOutput(PubKey pubkey1, PubKey pubkey2, ulong value)
    {
        Script redeemScript;
        if (pubkey1.CompareTo(pubkey2) < 0)
        {
            redeemScript = Create2of2MultisigRedeemScript(pubkey1, pubkey2);
        }
        else
        {
            redeemScript = Create2of2MultisigRedeemScript(pubkey2, pubkey1);
        }

        FundingScriptPubKey = PayToWitScriptHashTemplate.Instance.GenerateScriptPubKey(redeemScript.WitHash).ToBytes();
        Value = value;
    }

    private static Script Create2of2MultisigRedeemScript(PubKey pubkey1, PubKey pubkey2)
    {
        return new Script(
            Op.GetPushOp(2),
            Op.GetPushOp(pubkey1.ToBytes()),
            Op.GetPushOp(pubkey2.ToBytes()),
            Op.GetPushOp(2),
            OpcodeType.OP_CHECKMULTISIG
        );
    }
}