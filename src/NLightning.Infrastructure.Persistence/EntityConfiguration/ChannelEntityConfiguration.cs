using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NLightning.Domain.Transactions.Constants;

namespace NLightning.Infrastructure.Persistence.EntityConfiguration;

using Domain.Channels.Constants;
using Domain.Crypto.Constants;
using Domain.Protocol.Constants;
using Entities;
using Enums;

public static class ChannelEntityConfiguration
{
    public static void ConfigureChannelEntity(this ModelBuilder modelBuilder, DatabaseType databaseType)
    {
        modelBuilder.Entity<ChannelEntity>(entity =>
        {
            // Set PrimaryKey
            entity.HasKey(e => e.ChannelId);
            
            // Set required props
            entity.Property(e => e.FundingOutputIndex).IsRequired();
            entity.Property(e => e.FundingAmountSatoshis).IsRequired();
            entity.Property(e => e.IsInitiator).IsRequired();
            entity.Property(e => e.LocalCommitmentNumber).IsRequired();
            entity.Property(e => e.RemoteCommitmentNumber).IsRequired();
            entity.Property(e => e.LocalNextHtlcId).IsRequired();
            entity.Property(e => e.RemoteNextHtlcId).IsRequired();
            entity.Property(e => e.LocalRevocationNumber).IsRequired();
            entity.Property(e => e.RemoteRevocationNumber).IsRequired();
            entity.Property(e => e.State).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.LocalBalanceSatoshis).IsRequired();
            entity.Property(e => e.RemoteBalanceSatoshis).IsRequired();
            
            // Required byte[] properties
            entity.Property(e => e.ChannelId).IsRequired();
            entity.Property(e => e.FundingTxId).IsRequired();
            entity.Property(e => e.RemoteNodeId).IsRequired();
            
            // Nullable byte[] properties
            entity.Property(e => e.LastSentSignature).IsRequired(false);
            entity.Property(e => e.LastReceivedSignature).IsRequired(false);
            
            // Configure the relationship with ChannelConfig (1:1)
            entity.HasOne<ChannelConfigEntity>()
                .WithOne()
                .HasForeignKey<ChannelConfigEntity>(c => c.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure relationship with HTLCs (1:many)
            entity.HasMany(e => e.Htlcs)
                .WithOne()
                .HasForeignKey(h => h.ChannelId)
                .OnDelete(DeleteBehavior.Cascade);

            if (databaseType == DatabaseType.MicrosoftSql)
            {
                OptimizeConfigurationForSqlServer(entity);
            }
        });
    }
    
    private static void OptimizeConfigurationForSqlServer(EntityTypeBuilder<ChannelEntity> entity)
    {
        entity.Property(e => e.ChannelId).HasColumnType($"varbinary({ChannelConstants.ChannelIdLength})");
        entity.Property(e => e.FundingTxId).HasColumnType($"varbinary({TransactionConstants.TxIdLength})");
        entity.Property(e => e.RemoteNodeId).HasColumnType($"varbinary({TransactionConstants.TxIdLength})");
        entity.Property(e => e.LastSentSignature).HasColumnType($"varbinary({CryptoConstants.MaxSignatureSize})");
        entity.Property(e => e.LastReceivedSignature).HasColumnType($"varbinary({CryptoConstants.MaxSignatureSize})");
    }
}