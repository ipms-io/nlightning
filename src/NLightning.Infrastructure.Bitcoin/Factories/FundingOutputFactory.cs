using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories;

using Domain.Bitcoin.Factories;
using Domain.Bitcoin.Outputs;
using Domain.Money;
using Outputs;

public class FundingOutputFactory : IFundingOutputFactory
{
    public IFundingOutput Create(LightningMoney amount, ushort index, PubKey localPubKey, PubKey remotePubKey,
                                 uint256 txId)
    {
        return new FundingOutput(amount, localPubKey, remotePubKey)
        {
            Index = index,
            TxId = txId
        };
    }
}