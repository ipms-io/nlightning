using System.Collections.Immutable;
using Microsoft.EntityFrameworkCore;
using NLightning.Domain.Channels.Enums;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Channels.Models;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.Constants;
using NLightning.Domain.Crypto.Hashes;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Protocol.Interfaces;
using NLightning.Domain.Protocol.ValueObjects;
using NLightning.Domain.Serialization.Interfaces;
using NLightning.Domain.Transactions.Outputs;
using NLightning.Infrastructure.Persistence.Contexts;
using NLightning.Infrastructure.Persistence.Entities;

namespace NLightning.Infrastructure.Repositories.Database.Channels;

public class ChannelDbRepository : BaseDbRepository<ChannelEntity>, IChannelDbRepository
{
    IMessageSerializer _messageSerializer;
    private readonly ISecretStorageServiceFactory _secretStorageServiceFactory;
    ISha256 _sha256;
    
    public ChannelDbRepository(NLightningDbContext context, IMessageSerializer messageSerializer,
                               ISecretStorageServiceFactory secretStorageServiceFactory, ISha256 sha256) : base(context)
    {
        _messageSerializer = messageSerializer ?? throw new ArgumentNullException(nameof(messageSerializer));
        _secretStorageServiceFactory = secretStorageServiceFactory;
        _sha256 = sha256 ?? throw new ArgumentNullException(nameof(sha256));
    }
    
    public async Task AddAsync(Channel channel)
    {
        var channelEntity = await MapDomainToEntity(channel, _messageSerializer);
        Insert(channelEntity);
    }

    public async Task UpdateAsync(Channel channel)
    {
        var channelEntity = await MapDomainToEntity(channel, _messageSerializer);
        Update(channelEntity);
    }

    public Task DeleteAsync(ChannelId channelId)
    {
        return DeleteByIdAsync(channelId);
    }

    public async Task<Channel?> GetByIdAsync(ChannelId channelId)
    {
        var channelEntity = await DbSet
            .AsNoTracking()
            .Include(c => c.Config)
            .Include(c => c.KeySets)
            .Include(c => c.Htlcs)
            .FirstOrDefaultAsync(c => c.ChannelId == channelId);
        
        if (channelEntity is null)
            return null;

        return await MapEntityToDomain(channelEntity, _messageSerializer, _secretStorageServiceFactory, _sha256);
    }

    public async Task<IEnumerable<Channel>> GetAllAsync()
    {
        var channelEntities = await DbSet
            .AsNoTracking()
            .Include(c => c.Config)
            .Include(c => c.KeySets)
            .Include(c => c.Htlcs)
            .ToListAsync();
        
        return await Task.WhenAll(channelEntities.Select(
            async entity => await MapEntityToDomain(entity, _messageSerializer, _secretStorageServiceFactory,
                                                    _sha256)));
    }

    public async Task<IEnumerable<Channel>> GetReadyChannelsAsync()
    {
        byte[] readyStateList =
        [
            (byte)ChannelState.V1FundingCreated,
            (byte)ChannelState.V1FundingSigned,
            (byte)ChannelState.Open,
            (byte)ChannelState.Closing
        ];
        
        var channelEntities = await DbSet
            .AsNoTracking()
            .Include(c => c.Config)
            .Include(c => c.KeySets)
            .Include(c => c.Htlcs)
            .Where(c => readyStateList.Contains(c.State))
            .ToListAsync();
        
        return await Task.WhenAll(channelEntities.Select(
            async entity => await MapEntityToDomain(entity, _messageSerializer, _secretStorageServiceFactory,
                                                    _sha256)));
    }

    internal static async Task<ChannelEntity> MapDomainToEntity(Channel channel, IMessageSerializer messageSerializer)
    {
        var config = ChannelConfigDbRepository.MapDomainToEntity(channel.ChannelId, channel.ChannelConfig);
        ImmutableArray<ChannelKeySetEntity> keySets =
        [
            ChannelKeySetDbRepository.MapDomainToEntity(channel.ChannelId, true, channel.LocalKeySet),
            ChannelKeySetDbRepository.MapDomainToEntity(channel.ChannelId, false, channel.RemoteKeySet)
        ];
        
        var htlcs = new List<Htlc>();
        htlcs.AddRange(GetHtlcsOrNull(channel.LocalOfferedHtlcs));
        htlcs.AddRange(GetHtlcsOrNull(channel.LocalFulffiledHtlcs));
        htlcs.AddRange(GetHtlcsOrNull(channel.LocalOldHtlcs));
        htlcs.AddRange(GetHtlcsOrNull(channel.RemoteOfferedHtlcs));
        htlcs.AddRange(GetHtlcsOrNull(channel.RemoteFulffiledHtlcs));
        htlcs.AddRange(GetHtlcsOrNull(channel.RemoteOldHtlcs));
        
        List<HtlcEntity>? htlcEntities = null;
        if (htlcs.Count > 0)
        {
            htlcEntities = [];
            
            foreach (var htlc in htlcs)
                htlcEntities.Add(await HtlcDbRepository.MapDomainToEntityAsync(channel.ChannelId, htlc, messageSerializer));
        }
        
        return new ChannelEntity
        {
            ChannelId = channel.ChannelId,
            
            FundingTxId = channel.FundingOutput.TxId ?? new byte[CryptoConstants.Sha256HashLen],
            FundingOutputIndex = channel.FundingOutput.Index ?? 0,
            FundingAmountSatoshis = channel.FundingOutput.Amount.Satoshi,
            
            IsInitiator = channel.IsInitiator,
            RemoteNodeId = channel.RemoteNodeId,
            State = (byte)channel.State,
            Version = (byte)channel.Version,
            
            LocalBalanceSatoshis = channel.LocalBalance.Satoshi,
            RemoteBalanceSatoshis = channel.RemoteBalance.Satoshi,
            
            LocalCommitmentNumber = channel.LocalCommitmentNumber.Value,
            RemoteCommitmentNumber = channel.RemoteCommitmentNumber.Value,
            LocalNextHtlcId = channel.LocalNextHtlcId,
            RemoteNextHtlcId = channel.RemoteNextHtlcId,
            LocalRevocationNumber = channel.LocalRevocationNumber,
            RemoteRevocationNumber = channel.RemoteRevocationNumber,
            LastSentSignature = channel.LastSentSignature?.Value ?? null,
            LastReceivedSignature = channel.LastReceivedSignature?.Value ?? null,
            
            Config = config,
            KeySets = keySets,
            Htlcs = htlcEntities
        };
    }
    
    internal static async Task<Channel> MapEntityToDomain(ChannelEntity channelEntity,
                                                          IMessageSerializer messageSerializer,
                                                          ISecretStorageServiceFactory secretStorageServiceFactory,
                                                          ISha256 sha256)
    {
        if (channelEntity.Config is null)
            throw new InvalidOperationException(
                "Channel config cannot be null when mapping channel entity to domain model.");
        
        if (channelEntity.KeySets is not { Count: 2 })
            throw new InvalidOperationException(
                "Channel key sets must contain exactly two entries when mapping channel entity to domain model.");
        
        var localKeySetEntity = channelEntity.KeySets.FirstOrDefault(k => k.IsLocal);
        if (localKeySetEntity is null)
            throw new InvalidOperationException(
                "Local key set cannot be null when mapping channel entity to domain model.");
        
        var remoteKeySetEntity = channelEntity.KeySets.FirstOrDefault(k => !k.IsLocal);
        if (remoteKeySetEntity is null)
            throw new InvalidOperationException(
                "Remote key set cannot be null when mapping channel entity to domain model.");
        
        var config = ChannelConfigDbRepository.MapEntityToDomain(channelEntity.Config);
        var localKeySet = ChannelKeySetDbRepository.MapEntityToDomain(localKeySetEntity, secretStorageServiceFactory);
        var remoteKeySet = ChannelKeySetDbRepository.MapEntityToDomain(remoteKeySetEntity, secretStorageServiceFactory);

        var localOfferedHtlcs = new List<Htlc>();
        var localFulfilledHtlcs = new List<Htlc>();
        var localOldHtlcs = new List<Htlc>();
        var remoteOfferedHtlcs = new List<Htlc>();
        var remoteFulfilledHtlcs = new List<Htlc>();
        var remoteOldHtlcs = new List<Htlc>();
        if (channelEntity.Htlcs is { Count: > 0 })
        {
            foreach (var htlc in channelEntity.Htlcs.Where(h => h.State.Equals(HtlcState.Offered)))
            {
                var domainHtlc = await HtlcDbRepository.MapEntityToDomainAsync(htlc, messageSerializer);
                if (htlc.Direction.Equals(HtlcDirection.Outgoing))
                    localOfferedHtlcs.Add(domainHtlc);
                else
                    remoteOfferedHtlcs.Add(domainHtlc);
            }

            foreach (var htlc in channelEntity.Htlcs.Where(h => h.State.Equals(HtlcState.Fulfilled)))
            {
                var domainHtlc = await HtlcDbRepository.MapEntityToDomainAsync(htlc, messageSerializer);
                if (htlc.Direction.Equals(HtlcDirection.Outgoing))
                    localFulfilledHtlcs.Add(domainHtlc);
                else
                    remoteFulfilledHtlcs.Add(domainHtlc);
            }

            byte[] oldStates = [(byte)HtlcState.Expired, (byte)HtlcState.Failed];
            foreach (var htlc in channelEntity.Htlcs.Where(h => oldStates.Contains(h.State)))
            {
                var domainHtlc = await HtlcDbRepository.MapEntityToDomainAsync(htlc, messageSerializer);
                if (htlc.Direction.Equals(HtlcDirection.Outgoing))
                    localOldHtlcs.Add(domainHtlc);
                else
                    remoteOldHtlcs.Add(domainHtlc);
            }
        }
        
        var fundingOutput = new FundingOutputInfo(LightningMoney.Satoshis(channelEntity.FundingAmountSatoshis), 
                                                  localKeySet.FundingCompactPubKey, localKeySet.FundingCompactPubKey)
        {
            Index = channelEntity.FundingOutputIndex,
            TxId = channelEntity.FundingTxId
        };
        
        var localCommitmentNumber =
            new CommitmentNumber(localKeySet.PaymentCompactBasepoint, remoteKeySet.PaymentCompactBasepoint, sha256,
                                 channelEntity.LocalCommitmentNumber);
        var remoteCommitmentNumber =
            new CommitmentNumber(remoteKeySet.PaymentCompactBasepoint, localKeySet.PaymentCompactBasepoint, sha256,
                                 channelEntity.RemoteCommitmentNumber);
                                 
        CompactPubKey remoteNodeId = channelEntity.RemoteNodeId;

        DerSignature? lastSentSig = null;
        if (channelEntity.LastSentSignature != null)
            lastSentSig = new DerSignature(channelEntity.LastSentSignature);
            
        DerSignature? lastReceivedSig = null;
        if (channelEntity.LastReceivedSignature != null)
            lastReceivedSig = new DerSignature(channelEntity.LastReceivedSignature);
            
        return new Channel(
            channelId: new ChannelId(channelEntity.ChannelId),
            fundingOutput: fundingOutput,
            isInitiator: channelEntity.IsInitiator,
            remoteNodeId: remoteNodeId,
            channelConfig: config,
            state: (ChannelState)channelEntity.State,
            version: (ChannelVersion)channelEntity.Version,
            localBalance: LightningMoney.Satoshis(channelEntity.LocalBalanceSatoshis),
            remoteBalance: LightningMoney.Satoshis(channelEntity.RemoteBalanceSatoshis),
            localKeySet: localKeySet,
            remoteKeySet: remoteKeySet,
            localCommitmentNumber: localCommitmentNumber,
            remoteCommitmentNumber: remoteCommitmentNumber,
            localNextHtlcId: channelEntity.LocalNextHtlcId,
            remoteNextHtlcId: channelEntity.RemoteNextHtlcId,
            localRevocationNumber: channelEntity.LocalRevocationNumber,
            remoteRevocationNumber: channelEntity.RemoteRevocationNumber,
            lastSentSignature: lastSentSig,
            lastReceivedSignature: lastReceivedSig,
            localOfferedHtlcs: localOfferedHtlcs,
            localFulffiledHtlcs: localFulfilledHtlcs,
            localOldHtlcs: localOldHtlcs,
            remoteOfferedHtlcs: remoteOfferedHtlcs,
            remoteFulffiledHtlcs: remoteFulfilledHtlcs,
            remoteOldHtlcs: remoteOldHtlcs
        );
    }
    
    private static ICollection<Htlc> GetHtlcsOrNull(ICollection<Htlc>? htlcs)
    {
        return htlcs is { Count: > 0 } ? htlcs : [];
    }
}

