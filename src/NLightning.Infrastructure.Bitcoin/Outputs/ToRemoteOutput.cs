using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Domain.Money;

/// <summary>
/// Represents a to_remote output in a commitment transaction.
/// </summary>
public class ToRemoteOutput : BaseOutput
{
    private readonly bool _hasAnchorOutputs;

    public override ScriptType ScriptType => _hasAnchorOutputs
        ? ScriptType.P2WSH
        : ScriptType.P2WPKH;

    public PubKey RemotePubKey { get; }

    public ToRemoteOutput(bool hasAnchorOutputs, PubKey remotePubKey, LightningMoney amount)
        : base(GenerateToRemoteScript(hasAnchorOutputs, remotePubKey), amount)
    {
        ArgumentNullException.ThrowIfNull(remotePubKey);

        _hasAnchorOutputs = hasAnchorOutputs;

        RemotePubKey = remotePubKey;
    }

    private static Script GenerateToRemoteScript(bool hasAnchorOutputs, PubKey remotePubKey)
    {
        ArgumentNullException.ThrowIfNull(remotePubKey);

        if (hasAnchorOutputs)
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

        // If we don't require anchor outputs, we'll return a P2WPKH redeemScript
        return remotePubKey.WitHash.ScriptPubKey;
    }
}