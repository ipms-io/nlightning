using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

using Common.Managers;

/// <summary>
/// Represents a to_remote output in a commitment transaction.
/// </summary>
public class ToRemoteOutput : BaseOutput
{
    public override ScriptType ScriptType => ConfigManager.Instance.IsOptionAnchorOutput
        ? ScriptType.P2WSH
        : ScriptType.P2WPKH;

    public PubKey RemotePubKey { get; }

    public ToRemoteOutput(PubKey remotePubKey, LightningMoney amount)
        : base(GenerateToRemoteScript(remotePubKey), amount)
    {
        ArgumentNullException.ThrowIfNull(remotePubKey);

        RemotePubKey = remotePubKey;
    }

    private static Script GenerateToRemoteScript(PubKey remotePubKey)
    {
        ArgumentNullException.ThrowIfNull(remotePubKey);

        if (ConfigManager.Instance.IsOptionAnchorOutput)
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