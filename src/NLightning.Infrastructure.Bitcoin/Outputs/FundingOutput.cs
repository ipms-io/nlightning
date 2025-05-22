using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Domain.Bitcoin.Outputs;
using Domain.Money;

public class FundingOutput : BaseOutput, IFundingOutput
{
    public override ScriptType ScriptType => ScriptType.P2WSH;

    public PubKey LocalPubKey { get; }
    public PubKey RemotePubKey { get; }

    public FundingOutput(LightningMoney amount, PubKey localPubKey, PubKey remotePubKey, bool isPlaceHolder = false)
        : base(CreateMultisigScript(localPubKey, remotePubKey), amount)
    {
        ArgumentNullException.ThrowIfNull(localPubKey);
        ArgumentNullException.ThrowIfNull(remotePubKey);

        if (localPubKey == remotePubKey)
            throw new ArgumentException("Public keys must be different.");

        if (amount.IsZero)
            throw new ArgumentOutOfRangeException(nameof(amount), "Funding amount must be greater than zero.");

        LocalPubKey = localPubKey;
        RemotePubKey = remotePubKey;

        if (isPlaceHolder)
        {
            IsPlaceHolder = isPlaceHolder;
            Index = 0;
        }
    }

    private static Script CreateMultisigScript(PubKey localPubKey, PubKey remotePubKey)
    {
        ArgumentNullException.ThrowIfNull(localPubKey);
        ArgumentNullException.ThrowIfNull(remotePubKey);

        var orderedKeys = new[] { localPubKey, remotePubKey }.OrderBy(pk => pk, PubKeyComparer.Instance).ToArray();
        return PayToMultiSigTemplate.Instance.GenerateScriptPubKey(2, orderedKeys);
    }
}