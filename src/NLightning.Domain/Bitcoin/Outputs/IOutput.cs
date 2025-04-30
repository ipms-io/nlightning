using NBitcoin;

namespace NLightning.Domain.Bitcoin.Outputs;

public interface IOutput
{
    TxOut ToTxOut();
    ScriptCoin ToCoin();
    int CompareTo(IOutput? other);
}