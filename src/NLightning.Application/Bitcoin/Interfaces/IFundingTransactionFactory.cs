namespace NLightning.Application.Bitcoin.Interfaces;

using Domain.Bitcoin.Outputs;
using Domain.Bitcoin.Transactions;
using Domain.Bitcoin.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Money;

public interface IFundingTransactionFactory
{
    ITransaction CreateFundingTransaction(CompactPubKey localFundingCompactPubKey,
                                          CompactPubKey remoteFundingCompactPubKey, LightningMoney fundingSatoshis,
                                          BitcoinScript changeBitcoinScript, params IOutput[] outputs);

    ITransaction CreateFundingTransaction(CompactPubKey localFundingCompactPubKey,
                                          CompactPubKey remoteFundingCompactPubKey, LightningMoney fundingSatoshis,
                                          BitcoinScript redeemBitcoinScript, BitcoinScript changeBitcoinScript,
                                          params IOutput[] outputs);
}