namespace NLightning.Application.Bitcoin.Interfaces;

using Domain.Bitcoin.Transactions;
using Domain.Channels.Models;
using Domain.Channels.ValueObjects;
using Domain.Crypto.ValueObjects;
using Domain.Node.Options;

public interface ICommitmentTransactionService
{
    Task<ITransaction> CreateAndSignCommitmentTransactionAsync(ChannelModel channelModel,
                                                               IEnumerable<Htlc> htlcOutputsToInclude,
                                                               ulong localCommitmentNumber, bool isFunder,
                                                               NodeOptions nodeOptions,
                                                               CompactPubKey remoteCommitmentPoint);
}