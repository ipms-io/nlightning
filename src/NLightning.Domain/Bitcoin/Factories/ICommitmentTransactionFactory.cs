using NBitcoin;

namespace NLightning.Domain.Bitcoin.Factories;

using Infrastructure.Protocol.Models;
using Money;
using Node.Options;
using Outputs;
using Transactions;

public interface ICommitmentTransactionFactory
{
    ITransaction CreateCommitmentTransaction(NodeOptions nodeOptions, IOutput output, PubKey localPaymentBasepoint,
                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                             LightningMoney toRemoteAmount, CommitmentNumber commitmentNumber,
                                             bool isFunder, params BitcoinSecret[] secrets);

    ITransaction CreateCommitmentTransaction(NodeOptions nodeOptions, IOutput output, PubKey localPaymentBasepoint,
                                             PubKey remotePaymentBasepoint, PubKey localDelayedPubKey,
                                             PubKey revocationPubKey, LightningMoney toLocalAmount,
                                             LightningMoney toRemoteAmount, CommitmentNumber commitmentNumber,
                                             IEnumerable<IOutput> offeredHtlcs, IEnumerable<IOutput> receivedHtlcs,
                                             bool isFunder, params BitcoinSecret[] secrets);
}