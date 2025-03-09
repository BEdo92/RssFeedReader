using Microsoft.EntityFrameworkCore;
using RssFeedReader.Models;
using System.Text.Json;

namespace RssFeedReader.Data;

public class Seed
{
    public static async Task SeedFeedCources(FeedContext context)
    {
        // NOTE: To prevent duplicate data.
        if (await context.FeedSources.AnyAsync())
        {
            return;
        }

        string feedSourcesJson = await File.ReadAllTextAsync("Data/feedSources.json");

        JsonSerializerOptions options = new();

        List<FeedSource> feedSources = JsonSerializer.Deserialize<List<FeedSource>>(feedSourcesJson, options);

        if (feedSources is null)
        {
            return;
        }

        await context.FeedSources.AddRangeAsync(feedSources);
    }
}
