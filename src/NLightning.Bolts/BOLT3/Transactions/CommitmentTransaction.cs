using NBitcoin;
using NLightning.Bolts.BOLT3.Constants;
using NLightning.Common.Managers;

namespace NLightning.Bolts.BOLT3.Transactions;

/// <summary>
/// Represents a commitment transaction.
/// </summary>
public class CommitmentTransaction
{
    private readonly bool _isOptionAnchorOutputs;

    public Transaction Transaction { get; }
    public TxOut ToLocalOutput => Transaction.Outputs[0];
    public TxOut ToRemoteOutput => Transaction.Outputs[1];
    public TxOut? LocalAnchorOutput => _isOptionAnchorOutputs ? Transaction.Outputs[2] : null;
    public TxOut? RemoteAnchorOutput => _isOptionAnchorOutputs ? Transaction.Outputs[3] : null;
    public TxOut OfferedHtlcOutput => _isOptionAnchorOutputs ? Transaction.Outputs[4] : Transaction.Outputs[2];

    /// <summary>
    /// Initializes a new instance of the <see cref="CommitmentTransaction"/> class.
    /// </summary>
    /// <param name="fundingTxId">The funding transaction ID.</param>
    /// <param name="fundingOutputIndex">The index of the funding output.</param>
    /// <param name="fundingAmount">The amount of the funding output in satoshis.</param>
    /// <param name="localPubKey">The local public key.</param>
    /// <param name="remotePubKey">The remote public key.</param>
    /// <param name="localDelayedPubKey">The local delayed public key.</param>
    /// <param name="revocationPubKey">The revocation public key.</param>
    /// <param name="toLocalAmount">The amount for the to_local output in satoshis.</param>
    /// <param name="toRemoteAmount">The amount for the to_remote output in satoshis.</param>
    /// <param name="toSelfDelay">The to_self_delay in blocks.</param>
    /// <param name="obscuredCommitmentNumber">The obscured commitment number.</param>
    public CommitmentTransaction(
        uint256 fundingTxId,
        uint fundingOutputIndex,
        PubKey localPubKey,
        PubKey remotePubKey,
        PubKey localDelayedPubKey,
        PubKey revocationPubKey,
        Money toLocalAmount,
        Money toRemoteAmount,
        uint toSelfDelay,
        ulong obscuredCommitmentNumber,
        byte[] pubkey1Signature,
        byte[] pubkey2Signature)
    {
        _isOptionAnchorOutputs = ConfigManager.Instance.IsOptionAnchorOutput;
        Transaction = CreateTransaction(fundingTxId, fundingOutputIndex, localPubKey, remotePubKey, localDelayedPubKey, revocationPubKey, toLocalAmount, toRemoteAmount, toSelfDelay, obscuredCommitmentNumber, pubkey1Signature, pubkey2Signature);
    }

    public CommitmentTransaction(Transaction tx)
    {
        _isOptionAnchorOutputs = ConfigManager.Instance.IsOptionAnchorOutput;
        Transaction = tx;
    }

    private Transaction CreateTransaction(
        uint256 fundingTxId,
        uint fundingOutputIndex,
        PubKey localPubKey,
        PubKey remotePubKey,
        PubKey localDelayedPubKey,
        PubKey revocationPubKey,
        Money toLocalAmount,
        Money toRemoteAmount,
        uint toSelfDelay,
        ulong obscuredCommitmentNumber,
        byte[] pubkey1Signature,
        byte[] pubkey2Signature)
    {
        var tx = Transaction.Create(ConfigManager.Instance.Network);

        // Set version and locktime
        tx.Version = TransactionConstants.COMMITMENT_TRANSACTION_VERSION;
        tx.LockTime = new LockTime((0x20 << 24) | (uint)(obscuredCommitmentNumber & 0xFFFFFF));

        // Add the funding input
        var outpoint = new OutPoint(fundingTxId, fundingOutputIndex);
        var witScript = new WitScript(OpcodeType.OP_0, Op.GetPushOp(pubkey1Signature), Op.GetPushOp(pubkey2Signature));
        var sequence = new Sequence(0x80 << 24) | (uint)((obscuredCommitmentNumber >> 24) & 0xFFFFFF);
        tx.Inputs.Add(outpoint, null, witScript, sequence);

        // to_local output
        if (toLocalAmount >= new Money((long)546)) // Dust limit in satoshis
        {
            var toLocalScript = CreateToLocalScript(localDelayedPubKey, revocationPubKey, toSelfDelay);
            tx.Outputs.Add(new TxOut(toLocalAmount, toLocalScript.WitHash));
        }

        // to_remote output
        if (toRemoteAmount >= new Money((long)546)) // Dust limit in satoshis
        {
            var toRemoteScript = CreateToRemoteScript(remotePubKey);
            tx.Outputs.Add(new TxOut(toRemoteAmount, toRemoteScript.WitHash));
        }

        if (!_isOptionAnchorOutputs)
        {
            return tx;
        }

        // Local anchor output
        var localAnchorScript = CreateAnchorScript(localPubKey);
        tx.Outputs.Add(new TxOut(new Money((long)330), localAnchorScript.WitHash));

        // Remote anchor output
        var remoteAnchorScript = CreateAnchorScript(remotePubKey);
        tx.Outputs.Add(new TxOut(new Money((long)330), remoteAnchorScript.WitHash));

        return tx;
    }

    private static Script CreateToLocalScript(PubKey localDelayedPubKey, PubKey revocationPubKey, uint toSelfDelay)
    {
        /* The following script can be read as:
        ** spendingPubKey = the pubkey trying to sign this spend
        ** signature = the signature given by spendingPubKey
        ** nSequence = Provided by the spending transaction
        **
        ** if (revocationPubKey = spendingPubKey)
        ** { // Revocation key path
        **   return revocationPubKey
        ** }
        ** else
        ** { // Delayed key path
        **   if (nSequence < toSelfDelay)
        **   {
        **     exit
        **   }
        **   else
        **   {
        **     return localDelayedPubKey
        **   }
        ** }
        ** if (signature is valid for spendingPubKey)
        ** {
        **   return true
        ** }
        */

        return new Script(
            OpcodeType.OP_IF,
            Op.GetPushOp(revocationPubKey.ToBytes()),
            OpcodeType.OP_ELSE,
            Op.GetPushOp(toSelfDelay),
            OpcodeType.OP_CHECKSEQUENCEVERIFY,
            OpcodeType.OP_DROP,
            Op.GetPushOp(localDelayedPubKey.ToBytes()),
            OpcodeType.OP_ENDIF,
            OpcodeType.OP_CHECKSIG
        );
    }

    private Script CreateToRemoteScript(PubKey remotePubKey)
    {
        if (_isOptionAnchorOutputs)
        {
            /* The following script can be read as:
            ** spendingPubKey = the pubkey trying to sign this spend
            ** nSequence = Provided by the spending transaction
            ** 
            ** if (signature is valid for spendingPubKey && nSequence >= 1) {
            **     return true
            ** } else {
            **     return false
            ** }
            */
            return new Script(
                Op.GetPushOp(remotePubKey.ToBytes()),
                OpcodeType.OP_CHECKSIGVERIFY,
                OpcodeType.OP_1,
                OpcodeType.OP_CHECKSEQUENCEVERIFY
            );
        }

        // If we don't require anchor outputs, we can just return the remotePubKey
        return remotePubKey.WitHash.ScriptPubKey;
    }

    private static Script CreateAnchorScript(PubKey pubKey)
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