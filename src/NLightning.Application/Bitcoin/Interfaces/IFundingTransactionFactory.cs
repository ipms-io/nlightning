using NLightning.Domain.Bitcoin.Outputs;
using NLightning.Domain.Bitcoin.Transactions;
using NLightning.Domain.Bitcoin.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;

namespace NLightning.Application.Bitcoin.Interfaces;

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