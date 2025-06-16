using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NLightning.Infrastructure.Persistence.EntityConfiguration.Channel;

using Domain.Channels.Constants;
using Domain.Crypto.Constants;
using Entities.Channel;
using Enums;
using ValueConverters;

public static class ChannelKeySetEntityConfiguration
{
    public static void ConfigureChannelKeySetEntity(this ModelBuilder modelBuilder, DatabaseType databaseType)
    {
        modelBuilder.Entity<ChannelKeySetEntity>(entity =>
        {
            // Composite key
            entity.HasKey(e => new { e.ChannelId, e.IsLocal });

            // Set required props
            entity.Property(e => e.IsLocal).IsRequired();
            entity.Property(e => e.CurrentPerCommitmentIndex).IsRequired();
            entity.Property(e => e.KeyIndex).IsRequired();

            // Required byte[] properties
            entity.Property(e => e.ChannelId)
                  .HasConversion<ChannelIdConverter>()
                  .IsRequired();
            entity.Property(e => e.FundingPubKey).IsRequired();
            entity.Property(e => e.RevocationBasepoint).IsRequired();
            entity.Property(e => e.PaymentBasepoint).IsRequired();
            entity.Property(e => e.DelayedPaymentBasepoint).IsRequired();
            entity.Property(e => e.HtlcBasepoint).IsRequired();
            entity.Property(e => e.CurrentPerCommitmentPoint).IsRequired();

            // Nullable byte[] properties
            entity.Property(e => e.LastRevealedPerCommitmentSecret).IsRequired(false);

            if (databaseType == DatabaseType.MicrosoftSql)
                OptimizeConfigurationForSqlServer(entity);
        });
    }

    private static void OptimizeConfigurationForSqlServer(EntityTypeBuilder<ChannelKeySetEntity> entity)
    {
        entity.Property(e => e.ChannelId).HasColumnType($"varbinary({ChannelConstants.ChannelIdLength})");
        entity.Property(e => e.FundingPubKey).HasColumnType($"varbinary({CryptoConstants.CompactPubkeyLen})");
        entity.Property(e => e.RevocationBasepoint).HasColumnType($"varbinary({CryptoConstants.CompactPubkeyLen})");
        entity.Property(e => e.PaymentBasepoint).HasColumnType($"varbinary({CryptoConstants.CompactPubkeyLen})");
        entity.Property(e => e.DelayedPaymentBasepoint).HasColumnType($"varbinary({CryptoConstants.CompactPubkeyLen})");
        entity.Property(e => e.HtlcBasepoint).HasColumnType($"varbinary({CryptoConstants.CompactPubkeyLen})");
        entity.Property(e => e.CurrentPerCommitmentPoint)
              .HasColumnType($"varbinary({CryptoConstants.CompactPubkeyLen})");
    }
}