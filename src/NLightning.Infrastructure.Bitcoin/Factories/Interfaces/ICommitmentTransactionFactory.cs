using NBitcoin;

namespace NLightning.Infrastructure.Bitcoin.Factories.Interfaces;

using Domain.Bitcoin.Outputs;
using Domain.Bitcoin.Transactions;
using Domain.Money;
using Domain.Node.Options;
using Protocol.Models;

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