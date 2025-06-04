using NLightning.Domain.Channels.Enums;
using NLightning.Domain.Channels.Interfaces;
using NLightning.Domain.Channels.ValueObjects;
using NLightning.Domain.Crypto.ValueObjects;
using NLightning.Domain.Money;
using NLightning.Domain.Protocol.Messages;
using NLightning.Domain.Serialization.Interfaces;
using NLightning.Infrastructure.Persistence.Contexts;
using NLightning.Infrastructure.Persistence.Entities;

namespace NLightning.Infrastructure.Repositories.Database.Channels;

public class HtlcDbRepository : BaseDbRepository<HtlcEntity>, IHtlcDbRepository
{
    private readonly IMessageSerializer _messageSerializer;

    public HtlcDbRepository(NLightningDbContext context, IMessageSerializer messageSerializer) : base(context)
    {
        _messageSerializer = messageSerializer;
    }

    public async Task AddAsync(ChannelId channelId, Htlc htlc)
    {
        var htlcEntity = await MapDomainToEntityAsync(channelId, htlc, _messageSerializer);
        Insert(htlcEntity);
    }

    public async Task UpdateAsync(ChannelId channelId, Htlc htlc)
    {
        var htlcEntity = await MapDomainToEntityAsync(channelId, htlc, _messageSerializer);
        Update(htlcEntity);
    }

    public Task DeleteAsync(ChannelId channelId, ulong htlcId, HtlcDirection direction)
    {
        return DeleteByIdAsync((channelId, htlcId, (byte)direction));
    }

    public void DeleteAllForChannelId(ChannelId channelId)
    {
        DeleteWhere(h => h.ChannelId.Equals(channelId));
    }

    public async Task<Htlc?> GetByIdAsync(ChannelId channelId, ulong htlcId, HtlcDirection direction)
    {
        var htlcEntity = await base.GetByIdAsync((channelId, htlcId, (byte)direction));

        if (htlcEntity == null)
            return null;

        return await MapEntityToDomainAsync(htlcEntity, _messageSerializer);
    }

    public async Task<IEnumerable<Htlc>> GetAllForChannelAsync(ChannelId channelId)
    {
        var htlcEntities = Get(h => h.ChannelId.Equals(channelId)).ToList();

        return await Task.WhenAll(htlcEntities.Select(h => MapEntityToDomainAsync(h, _messageSerializer)));
    }

    public async Task<IEnumerable<Htlc>> GetByChannelIdAndStateAsync(ChannelId channelId, HtlcState state)
    {
        var htlcEntities = Get(h => h.ChannelId.Equals(channelId) && h.State.Equals(state)).ToList();

        return await Task.WhenAll(htlcEntities.Select(h => MapEntityToDomainAsync(h, _messageSerializer)));
    }

    public async Task<IEnumerable<Htlc>> GetByChannelIdAndDirectionAsync(ChannelId channelId, HtlcDirection direction)
    {
        var htlcEntities = Get(h => h.ChannelId.Equals(channelId) && h.Direction.Equals(direction)).ToList();

        return await Task.WhenAll(htlcEntities.Select(h => MapEntityToDomainAsync(h, _messageSerializer)));
    }

    internal static async Task<HtlcEntity> MapDomainToEntityAsync(ChannelId channelId, Htlc htlc,
                                                                  IMessageSerializer messageSerializer)
    {
        using var stream = new MemoryStream();
        await messageSerializer.SerializeAsync(htlc.AddMessage, stream);

        return new HtlcEntity
        {
            ChannelId = channelId,
            HtlcId = htlc.Id,
            AmountMsat = htlc.Amount.MilliSatoshi,
            PaymentHash = htlc.PaymentHash,
            CltvExpiry = htlc.CltvExpiry,
            State = (byte)htlc.State,
            Direction = (byte)htlc.Direction,
            ObscuredCommitmentNumber = htlc.ObscuredCommitmentNumber,
            AddMessageBytes = stream.ToArray(),
            PaymentPreimage = htlc.PaymentPreimage
        };
    }

    internal static async Task<Htlc> MapEntityToDomainAsync(HtlcEntity htlcEntity, IMessageSerializer messageSerializer)
    {
        Hash? paymentPreimage = null;
        if (htlcEntity.PaymentPreimage is not null)
            paymentPreimage = htlcEntity.PaymentPreimage;

        CompactSignature? signature = null;
        if (htlcEntity.Signature is not null)
            signature = htlcEntity.Signature;

        using var stream = new MemoryStream(htlcEntity.AddMessageBytes);
        var addMessage = await messageSerializer.DeserializeMessageAsync<UpdateAddHtlcMessage>(stream);
        if (addMessage is null)
            throw new InvalidOperationException("Failed to deserialize HTLC add message");

        return new Htlc(LightningMoney.MilliSatoshis(htlcEntity.AmountMsat), addMessage,
                        (HtlcDirection)htlcEntity.Direction, htlcEntity.CltvExpiry, htlcEntity.HtlcId,
                        htlcEntity.ObscuredCommitmentNumber, htlcEntity.PaymentHash,
                        (HtlcState)htlcEntity.State, paymentPreimage, signature);
    }
}