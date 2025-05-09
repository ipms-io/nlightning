using Microsoft.EntityFrameworkCore;

namespace NLightning.Infrastructure.Persistence.Contexts;

using Entities;

public class NLightningContext : DbContext
{
    public NLightningContext(DbContextOptions<NLightningContext> options)
        : base(options)
    {
    }

    public DbSet<Node> Nodes { get; set; }
    public DbSet<ChannelKeyDataEntity> ChannelKeyData { get; set; }

    [PrimaryKey(nameof(Id))]
    public class Node
    {
        public long Id { get; set; }
    }
}