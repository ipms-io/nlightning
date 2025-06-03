using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.Constants;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Infrastructure.Persistence.Contexts;
using NLightning.Infrastructure.Persistence.Entities;

namespace NLightning.Infrastructure.Repositories.Database.Channels;

public class ChannelKeySetDbRepository : BaseDbRepository<ChannelKeySetEntity>, IChannelKeySetDbRepository
{
    private readonly ISecretStorageServiceFactory _secretStorageServiceFactory;

    public ChannelKeySetDbRepository(NLightningDbContext context,
                                     ISecretStorageServiceFactory secretStorageServiceFactory) : base(context)
    {
        _secretStorageServiceFactory = secretStorageServiceFactory
                                       ?? throw new ArgumentNullException(nameof(secretStorageServiceFactory));
    }

    public void Add(ChannelId channelId, bool isLocal, ChannelKeySet keySet)
    {
        var keySetEntity = MapDomainToEntity(channelId, isLocal, keySet);
        Insert(keySetEntity);
    }

    public void Update(ChannelId channelId, bool isLocal, ChannelKeySet keySet)
    {
        var keySetEntity = MapDomainToEntity(channelId, isLocal, keySet);
        base.Update(keySetEntity);
    }

    public Task DeleteAsync(ChannelId channelId, bool isLocal)
    {
        return DeleteByIdAsync((channelId, isLocal));
    }

    public async Task<ChannelKeySet?> GetByIdAsync(ChannelId channelId, bool isLocal)
    {
        var keySetEntity = await base.GetByIdAsync((channelId, isLocal));
            
        if (keySetEntity is null)
            return null;

        return MapEntityToDomain(keySetEntity, _secretStorageServiceFactory);
    }

    internal static ChannelKeySetEntity MapDomainToEntity(ChannelId channelId, bool isLocal, ChannelKeySet keySet)
    {
        byte[]? lastPerCommitmentSecret = null;
        if (keySet.CurrentPerCommitmentIndex < CryptoConstants.FirstPerCommitmentIndex)
            lastPerCommitmentSecret = keySet.SecretStorageService.DeriveOldSecret(keySet.CurrentPerCommitmentIndex + 1);

        return new ChannelKeySetEntity
        {
            ChannelId = channelId,
            IsLocal = isLocal,
            
            FundingPubKey = keySet.FundingCompactPubKey,
            RevocationBasepoint = keySet.RevocationCompactBasepoint,
            PaymentBasepoint = keySet.PaymentCompactBasepoint,
            DelayedPaymentBasepoint = keySet.DelayedPaymentCompactBasepoint,
            HtlcBasepoint = keySet.HtlcCompactBasepoint,
            GossipPubKey = keySet.GossipCompactPubkey,
            CurrentPerCommitmentPoint = keySet.CurrentPerCommitmentCompactPoint,
            CurrentPerCommitmentIndex = keySet.CurrentPerCommitmentIndex,
            KeyIndex = keySet.KeyIndex,
            LastPerCommitmentSecret = lastPerCommitmentSecret
        };
    }

    internal static ChannelKeySet MapEntityToDomain(ChannelKeySetEntity entity,
                                                    ISecretStorageServiceFactory secretStorageServiceFactory)
    {
        CompactPubKey? gossipCompactPubkey = null;
        if (entity.GossipPubKey is not null)
            gossipCompactPubkey = new CompactPubKey(entity.GossipPubKey);

        var secretStorageService = secretStorageServiceFactory.CreatePerCommitmentStorage();
        if (entity.LastPerCommitmentSecret is not null)
            secretStorageService.InsertSecret(entity.LastPerCommitmentSecret, entity.CurrentPerCommitmentIndex + 1);

        return new ChannelKeySet(
            fundingCompactPubKey: entity.FundingPubKey,
            revocationCompactBasepoint: entity.RevocationBasepoint,
            paymentCompactBasepoint: entity.PaymentBasepoint,
            delayedPaymentCompactBasepoint: entity.DelayedPaymentBasepoint,
            htlcCompactBasepoint: entity.HtlcBasepoint,
            gossipCompactPubkey: gossipCompactPubkey,
            currentPerCommitmentCompactPoint: entity.CurrentPerCommitmentPoint,
            currentPerCommitmentIndex: entity.CurrentPerCommitmentIndex,
            keyIndex: entity.KeyIndex,
            secretStorageService: secretStorageService // This should be properly set up with your actual implementation
        );
    }
}
