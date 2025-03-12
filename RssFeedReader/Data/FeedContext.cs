using Microsoft.EntityFrameworkCore;
using RssFeedReader.Models;

namespace RssFeedReader.Data;

public class FeedContext(DbContextOptions<FeedContext> options) : DbContext(options)
{
    public DbSet<FeedSource> FeedSources { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<Statistics> Statistics { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>()
            .Property(e => e.Id)
            .ValueGeneratedOnAdd();

        base.OnModelCreating(modelBuilder);
    }
}
