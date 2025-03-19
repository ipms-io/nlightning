using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

using Common.Managers;

/// <summary>
/// Represents a to_remote output in a commitment transaction.
/// </summary>
public class ToRemoteOutput : OutputBase
{
    public PubKey RemotePubKey { get; }

    public ToRemoteOutput(PubKey remotePubKey, LightningMoney amountSats)
        : base(GenerateToRemoteScript(remotePubKey), amountSats)
    {
        RemotePubKey = remotePubKey;
    }

    private static Script GenerateToRemoteScript(PubKey remotePubKey)
    {
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

        // If we don't require anchor outputs, we can just return the remotePubKey
        return remotePubKey.WitHash.ScriptPubKey;
    }
}