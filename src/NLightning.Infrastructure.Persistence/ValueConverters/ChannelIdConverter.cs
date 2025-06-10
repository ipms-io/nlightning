using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NLightning.Infrastructure.Persistence.ValueConverters;

using Domain.Channels.ValueObjects;

/// <summary>
/// EF Core value converter for ChannelId value object
/// </summary>
public class ChannelIdConverter : ValueConverter<ChannelId, byte[]>

{
    public ChannelIdConverter() : base(channelId => channelId, bytes => new ChannelId(bytes))
    {
    }
}