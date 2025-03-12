using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

/// <summary>
/// Represents a to_local_anchor/to_remote_anchor output in a commitment transaction.
/// </summary>
public class ToAnchorOutput : OutputBase
{
    public PubKey RemoteFundingPubKey { get; set; }
    public ToAnchorOutput(PubKey remoteFundingPubKey, ulong amountSats)
        : base(GenerateAnchorScript(remoteFundingPubKey), amountSats)
    {
        RemoteFundingPubKey = remoteFundingPubKey;
    }

    /// <summary>
    /// Creates a ToAnchorOutput object from a NBitcoin.TxOut.
    /// </summary>
    /// <param name="txOut">The TxOut object.</param>
    /// <param name="remoteFundingPubKey">The remote funding public key.</param>
    /// <returns>A ToAnchorOutput object.</returns>
    public static ToAnchorOutput FromTxOut(TxOut txOut, PubKey remoteFundingPubKey)
    {
        return new ToAnchorOutput(remoteFundingPubKey, (ulong)txOut.Value.Satoshi);
    }
    
    private static Script GenerateAnchorScript(PubKey pubKey)
    {
        /* The following script can be read as:
         ** spendingPubKey = the pubkey trying to sign this spend
         ** signature = the signature given by spendingPubKey
         ** nSequence = Provided by the spending transaction
         **
         ** if (signature is valid for local_funding_pubkey/remote_funding_pubkey)
         ** {
         **     return true
         ** }
         ** else
         ** {
         **     if (currentTransactionInputSequence < 16)
         **     {
         **         exit
         **     }
         ** }
         ** return true
         */

        return new Script(
            Op.GetPushOp(pubKey.ToBytes()),
            OpcodeType.OP_CHECKSIG,
            OpcodeType.OP_IFDUP,
            OpcodeType.OP_NOTIF,
            OpcodeType.OP_16,
            OpcodeType.OP_CHECKSEQUENCEVERIFY,
            OpcodeType.OP_ENDIF
        );
    }
}