namespace NLightning.Domain.Node.Interfaces;

using Crypto.ValueObjects;
using Models;

public interface IPeerDbRepository
{
    Task AddOrUpdateAsync(PeerModel peerModel);
    void Update(PeerModel peerModel);
    Task<IEnumerable<PeerModel>> GetAllAsync();
    Task<PeerModel?> GetByNodeIdAsync(CompactPubKey nodeId);
    Task UpdatePeerLastSeenAsync(CompactPubKey peerCompactPubKey);
}