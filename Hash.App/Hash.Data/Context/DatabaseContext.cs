using Hash.Data.Context.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hash.Data.Context;

public partial class DatabaseContext : DbContext
{
    public DatabaseContext() {}
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    public DbSet<HashModel> Hashes { get; set; }
    public DbSet<HashAnalyticsModel> HashAnalytics { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}