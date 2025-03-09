using Microsoft.EntityFrameworkCore;
using RssFeedReader.Models;

namespace RssFeedReader.Data;

public class FeedContext(DbContextOptions<FeedContext> options) : DbContext(options)
{
    public DbSet<FeedSource> FeedSources { get; set; }
    public DbSet<News> News { get; set; }
}
