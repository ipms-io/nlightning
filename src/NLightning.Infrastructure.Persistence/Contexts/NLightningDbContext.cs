using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Persistence.Contexts;

using Entities;
using EntityConfiguration;
using Enums;
using Providers;

public class NLightningDbContext : DbContext
{
    private readonly DatabaseType _databaseType;

    public NLightningDbContext(DbContextOptions<NLightningDbContext> options, DatabaseTypeProvider databaseTypeProvider)
        : base(options)
    {
        _databaseType = databaseTypeProvider.DatabaseType;
    }

    public DbSet<ChannelEntity> Channels { get; set; }
    public DbSet<ChannelConfigEntity> ChannelConfigs { get; set; }
    public DbSet<ChannelKeySetEntity> ChannelKeySets { get; set; }
    public DbSet<HtlcEntity> Htlcs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ConfigureChannelEntity(_databaseType);
        modelBuilder.ConfigureChannelConfigEntity(_databaseType);
        modelBuilder.ConfigureChannelKeySetEntity(_databaseType);
        modelBuilder.ConfigureHtlcEntity(_databaseType);
    }
}