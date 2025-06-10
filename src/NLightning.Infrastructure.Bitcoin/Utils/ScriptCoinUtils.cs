using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Utils;

using Domain.Bitcoin.Outputs;

public static class ScriptCoinUtils
{
    public static ScriptCoin CreateScriptCoinFromOutput(IOutput output)
    {
        if (output.TransactionId.IsZero || output.TransactionId.IsOne)
            throw new InvalidOperationException("Transaction ID is not set. Sign the transaction first.");

        if (output.Amount.IsZero)
            throw new InvalidOperationException("You can't spend a zero amount output.");

        return new ScriptCoin(new uint256(output.TransactionId), output.Index, Money.Satoshis(output.Amount.Satoshi),
                              new Script(output.BitcoinScriptPubKey), new Script(output.RedeemBitcoinScript));
    }
}