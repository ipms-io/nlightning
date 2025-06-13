using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NLightning.Infrastructure.Persistence.EntityConfiguration.Bitcoin;

using Domain.Crypto.Constants;
using Entities.Bitcoin;
using Enums;
using ValueConverters;

public static class BlockchainStateEntityConfiguration
{
    public static void ConfigureBlockchainStateEntity(this ModelBuilder modelBuilder, DatabaseType databaseType)
    {
        modelBuilder.Entity<BlockchainStateEntity>(entity =>
        {
            // Set PrimaryKey
            entity.HasKey(e => e.Id);

            // Set required props
            entity.Property(e => e.LastProcessedHeight).IsRequired();
            entity.Property(e => e.LastProcessedBlockHash)
                  .HasConversion<HashConverter>()
                  .IsRequired();
            entity.Property(e => e.LastProcessedAt).IsRequired();

            // Configure Id as Guid
            entity.Property(e => e.Id)
                  .HasConversion<Guid>()
                  .IsRequired();

            if (databaseType == DatabaseType.MicrosoftSql)
                OptimizeConfigurationForSqlServer(entity);
        });
    }

    private static void OptimizeConfigurationForSqlServer(EntityTypeBuilder<BlockchainStateEntity> entity)
    {
        entity.Property(e => e.LastProcessedBlockHash).HasColumnType($"varbinary({CryptoConstants.Sha256HashLen})");
    }
}