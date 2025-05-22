using NBitcoin;

namespace NLightning.Domain.Bitcoin.Factories;

using Money;
using Outputs;

public interface IFundingOutputFactory
{
    IFundingOutput Create(LightningMoney amount, ushort index, PubKey localPubKey, PubKey remotePubKey, uint256 txId);
}