using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Outputs;

using Domain.Money;

public class ChangeOutput : BaseOutput
{
    public override ScriptType ScriptType => ScriptType.P2WPKH;

    public ChangeOutput(Script scriptPubKey, LightningMoney? amountSats = null) : base(scriptPubKey, amountSats ?? 0UL)
    { }
    public ChangeOutput(Script redeemScript, Script scriptPubKey, LightningMoney? amountSats = null)
        : base(redeemScript, scriptPubKey, amountSats ?? 0UL)
    { }
}