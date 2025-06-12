using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace NLightning.Infrastructure.Persistence.ValueConverters;

using Domain.Channels.ValueObjects;

/// <summary>
/// EF Core value converter for ChannelId value object
/// </summary>
public class ChannelIdConverter()
    : ValueConverter<ChannelId, byte[]>(channelId => channelId, bytes => new ChannelId(bytes));