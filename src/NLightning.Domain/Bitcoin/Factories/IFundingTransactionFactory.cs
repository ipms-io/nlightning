using NBitcoin;

namespace NLightning.Domain.Bitcoin.Factories;

using Money;
using Transactions;

public interface IFundingTransactionFactory
{
    ITransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                          LightningMoney fundingSatoshis, Script changeScript, Coin[] coins,
                                          params BitcoinSecret[] secrets);

    ITransaction CreateFundingTransaction(PubKey localFundingPubKey, PubKey remoteFundingPubKey,
                                          LightningMoney fundingSatoshis, Script redeemScript, Script changeScript,
                                          Coin[] coins, params BitcoinSecret[] secrets);
}