using RssFeedReader.Data;
using RssFeedReader.Models;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RssFeedReader.Services;

public class RssFeedService(IServiceScopeFactory scopeFactory, ILogger<RssFeedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Fetching RSS feeds...");

            using (var scope = scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<FeedContext>();
                XmlReader reader = XmlReader.Create("http://localhost:8000/BlogService/GetBlog");
                SyndicationFeed feed = SyndicationFeed.Load(reader);
                dbContext.News.Add(new News
                {
                    Title = feed.Title.Text,
                    Description = feed.Description.Text,
                    PublishDate = feed.LastUpdatedTime.DateTime,
                    Author = feed.Authors[0].Name,
                    Url = feed.Links[0].Uri.ToString(),
                    Categories = string.Join(", ", feed.Categories.Select(c => c.Name)),
                    Contributors = string.Join(", ", feed.Contributors.Select(c => c.Name)),
                    Copyright = feed.Copyright?.Text,
                    Generator = feed.Generator,
                    ImageUrl = feed.ImageUrl?.ToString(),
                    Language = feed.Language,
                    LastUpdatedTime = feed.LastUpdatedTime.DateTime
                });
                reader.Close();
            }

            //await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            await Task.Delay(1_000, stoppingToken);
        }
    }
}
