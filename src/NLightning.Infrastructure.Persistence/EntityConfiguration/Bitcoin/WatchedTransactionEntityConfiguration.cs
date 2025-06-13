using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NLightning.Infrastructure.Persistence.EntityConfiguration.Bitcoin;

using Domain.Channels.Constants;
using Domain.Crypto.Constants;
using Entities.Bitcoin;
using Enums;
using ValueConverters;

public static class WatchedTransactionEntityConfiguration
{
    public static void ConfigureWatchedTransactionEntity(this ModelBuilder modelBuilder, DatabaseType databaseType)
    {
        modelBuilder.Entity<WatchedTransactionEntity>(entity =>
        {
            // Set PrimaryKey
            entity.HasKey(e => new { e.TransactionId });

            // Set required props
            entity.Property(e => e.TransactionId)
                  .HasConversion<TxIdConverter>()
                  .IsRequired();
            entity.Property(e => e.ChannelId)
                  .HasConversion<ChannelIdConverter>()
                  .IsRequired();
            entity.Property(e => e.RequiredDepth).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Configure optional props
            entity.Property(e => e.FirstSeenAtHeight).IsRequired(false);
            entity.Property(e => e.CompletedAt).IsRequired(false);

            if (databaseType == DatabaseType.MicrosoftSql)
                OptimizeConfigurationForSqlServer(entity);
        });
    }

    private static void OptimizeConfigurationForSqlServer(EntityTypeBuilder<WatchedTransactionEntity> entity)
    {
        entity.Property(e => e.ChannelId).HasColumnType($"varbinary({ChannelConstants.ChannelIdLength})");
        entity.Property(e => e.TransactionId).HasColumnType($"varbinary({CryptoConstants.Sha256HashLen})");
    }
}