using NBitcoin;
using NLightning.Domain.Money;

namespace NLightning.Domain.Bitcoin.Outputs;

public interface IOutput
{
    LightningMoney Amount { get; }

    TxOut ToTxOut();
    ScriptCoin ToCoin();
    int CompareTo(IOutput? other);
}