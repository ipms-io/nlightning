using NLightning.Domain.Bitcoin.Transactions;
using NLightning.Domain.Channels;
using NLightning.Domain.Channels.Models;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Node.Options;
using NLightning.Domain.Protocol.Models;

namespace NLightning.Application.Bitcoin.Interfaces;

public interface ICommitmentTransactionService
{
    Task<ITransaction> CreateAndSignCommitmentTransactionAsync(Channel channel, IEnumerable<Htlc> htlcOutputsToInclude,
                                                               ulong localCommitmentNumber, bool isFunder,
                                                               NodeOptions nodeOptions,
                                                               CompactPubKey remoteCommitmentPoint);
}