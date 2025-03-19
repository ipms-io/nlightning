using NBitcoin;

namespace NLightning.Bolts.BOLT3.Outputs;

public class FundingOutput : BaseOutput
{
    public override ScriptType ScriptType => ScriptType.P2WSH;

    public PubKey LocalPubKey { get; }
    public PubKey RemotePubKey { get; }

    public FundingOutput(PubKey localPubKey, PubKey remotePubKey, LightningMoney amountSats)
        : base(CreateMultisigScript(localPubKey, remotePubKey), amountSats)
    {
        ArgumentNullException.ThrowIfNull(localPubKey);
        ArgumentNullException.ThrowIfNull(remotePubKey);

        if (localPubKey == remotePubKey)
            throw new ArgumentException("Public keys must be different.");

        if (amountSats.IsZero)
            throw new ArgumentException("Funding amount must be greater than zero.");

        LocalPubKey = localPubKey;
        RemotePubKey = remotePubKey;
    }

    private static Script CreateMultisigScript(PubKey localPubKey, PubKey remotePubKey)
    {
        var orderedKeys = new[] { localPubKey, remotePubKey }.OrderBy(pk => pk, PubKeyComparer.Instance).ToArray();
        return PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, orderedKeys);
    }
}