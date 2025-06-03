using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLightning.Domain.Transactions.Constants;

namespace NLightning.Infrastructure.Persistence.EntityConfiguration;

using Domain.Channels.Constants;
using Domain.Protocol.Constants;
using Entities;
using Enums;

public static class HtlcEntityConfiguration
{
    public static void ConfigureHtlcEntity(this ModelBuilder modelBuilder, DatabaseType databaseType)
    {
        modelBuilder.Entity<HtlcEntity>(entity =>
        {
            // Configure the composite key using ChannelId, HtlcId, and Direction
            entity.HasKey(h => new { h.ChannelId, h.HtlcId, h.Direction });
            
            // Set required props
            entity.Property(e => e.HtlcId).IsRequired();
            entity.Property(e => e.Direction).IsRequired();
            entity.Property(e => e.AmountMsat).IsRequired();
            entity.Property(e => e.CltvExpiry).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.ObscuredCommitmentNumber).IsRequired();
            
            // Required byte[] properties
            entity.Property(h => h.ChannelId).IsRequired();
            entity.Property(h => h.PaymentHash).IsRequired();
            entity.Property(h => h.AddMessageBytes).IsRequired();
            
            // Nullable byte[] properties
            entity.Property(h => h.PaymentPreimage).IsRequired(false);
            entity.Property(h => h.Signature).IsRequired(false);
            
            if (databaseType == DatabaseType.MicrosoftSql)
            {
                OptimizeConfigurationForSqlServer(entity);
            }
        });
    }
    
    private static void OptimizeConfigurationForSqlServer(EntityTypeBuilder<HtlcEntity> entity)
    {
        entity.Property(h => h.ChannelId).HasColumnType($"varbinary({ChannelConstants.ChannelIdLength})");
        entity.Property(h => h.PaymentHash).HasColumnType($"varbinary({TransactionConstants.TxIdLength})");
        entity.Property(h => h.PaymentPreimage).HasColumnType($"varbinary({TransactionConstants.TxIdLength})");
        entity.Property(h => h.AddMessageBytes).HasColumnType("varbinary(max)");
    }
}
