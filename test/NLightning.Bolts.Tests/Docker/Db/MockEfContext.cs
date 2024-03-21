using Microsoft.EntityFrameworkCore;

namespace NLightning.Bolts.Tests.Db;

public class MockEfContext : DbContext
{
    public MockEfContext(DbContextOptions<MockEfContext> options)
        : base(options)
    {
    }

    public DbSet<TableX> Xs { get; set; }

    public class TableX
    {
        public int Id { get; set; }
        public string? ShowCasingTransform { get; set; }
    }
}