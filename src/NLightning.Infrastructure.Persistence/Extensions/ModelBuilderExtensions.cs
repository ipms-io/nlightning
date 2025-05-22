using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Persistence.Extensions;

using Entities;

public static class ModelBuilderExtensions
{
    public static void ConfigureChannelKeyData(this ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ChannelKeyDataEntity>();

        entity.ToTable("channel_key_data");
        entity.HasKey(e => e.Id);

        // Configure column types for different databases
        entity.Property(e => e.ChannelId)
            .HasColumnName("channel_id")
            .IsRequired();

        entity.Property(e => e.KeyIndex)
            .HasColumnName("key_index")
            .IsRequired();

        // Similar configuration for other properties
    }
}