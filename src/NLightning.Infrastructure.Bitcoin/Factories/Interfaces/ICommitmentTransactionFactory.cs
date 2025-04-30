using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories.Interfaces;

using Domain.Bitcoin.Outputs;
using Domain.Bitcoin.Transactions;
using Domain.Money;
using Protocol.Models;

public interface ICommitmentTransactionFactory
{
    ITransaction CreateCommitmentTransaction(IOutput output, PubKey localPaymentBasepoint,
                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                             params BitcoinSecret[] secrets);

    ITransaction CreateCommitmentTransaction(IOutput output, PubKey localPaymentBasepoint,
                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                             LightningMoney toRemoteAmount, uint toSelfDelay,
                                             CommitmentNumber commitmentNumber, bool isChannelFunder,
                                             IEnumerable<IOutput> offeredHtlcs, IEnumerable<IOutput> receivedHtlcs,
                                             params BitcoinSecret[] secrets);
}