using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NLightning.Infrastructure.Persistence.EntityConfiguration;

using Domain.Channels.Constants;
using Entities;
using Enums;

public static class ChannelConfigEntityConfiguration
{
    public static void ConfigureChannelConfigEntity(this ModelBuilder modelBuilder, DatabaseType databaseType)
    {
        modelBuilder.Entity<ChannelConfigEntity>(entity =>
        {
            // Set PrimaryKey
            entity.HasKey(e => e.ChannelId);
            
            // Set required props
            entity.Property(e => e.ChannelId).IsRequired();
            entity.Property(e => e.MinimumDepth).IsRequired();
            entity.Property(e => e.ToSelfDelay).IsRequired();
            entity.Property(e => e.MaxAcceptedHtlcs).IsRequired();
            entity.Property(e => e.LocalDustLimitAmountSats).IsRequired();
            entity.Property(e => e.RemoteDustLimitAmountSats).IsRequired();
            entity.Property(e => e.HtlcMinimumMsat).IsRequired();
            entity.Property(e => e.MaxHtlcAmountInFlight).IsRequired();
            entity.Property(e => e.FeeRatePerKwSatoshis).IsRequired();
            entity.Property(e => e.OptionAnchorOutputs).IsRequired();
            
            // Nullable byte[] properties
            entity.Property(e => e.LocalUpfrontShutdownScript).IsRequired(false);
            entity.Property(e => e.RemoteUpfrontShutdownScript).IsRequired(false);

            if (databaseType == DatabaseType.MicrosoftSql)
            {
                OptimizeConfigurationForSqlServer(entity);
            }
        });
    }
    
    private static void OptimizeConfigurationForSqlServer(EntityTypeBuilder<ChannelConfigEntity> entity)
    {
        entity.Property(e => e.ChannelId).HasColumnType($"varbinary({ChannelConstants.ChannelIdLength})");
        entity.Property(e => e.LocalUpfrontShutdownScript).HasColumnType("varbinary(max)");
        entity.Property(e => e.RemoteUpfrontShutdownScript).HasColumnType("varbinary(max)");
    }
}
