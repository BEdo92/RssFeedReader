using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;
using RssFeedReader.Models;
using System.ServiceModel.Syndication;
using System.Xml;

namespace RssFeedReader.Services;

public class RssFeedService : BackgroundService
{
    private readonly HttpClient httpClient;
    private readonly IServiceScopeFactory scopeFactory;
    private readonly ILogger<RssFeedService> logger;

    public RssFeedService(IServiceScopeFactory scopeFactory, ILogger<RssFeedService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;

        HttpClientHandler handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        httpClient = new HttpClient(handler);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Fetching RSS feeds...");

                using IServiceScope scope = scopeFactory.CreateScope();

                FeedContext dbContext = scope.ServiceProvider.GetRequiredService<FeedContext>();
                var feedSources = await dbContext.FeedSources.ToListAsync(stoppingToken);

                foreach (FeedSource? feedSource in feedSources)
                {
                    var lastNews = await dbContext.News
                        .Where(n => n.FeedSourceId == feedSource.Id)
                        .OrderByDescending(n => n.PublishDate)
                        .FirstOrDefaultAsync(stoppingToken);

                    var request = new HttpRequestMessage(HttpMethod.Get, feedSource.Url);
                    if (lastNews != null)
                    {
                        request.Headers.IfModifiedSince = lastNews.PublishDate;
                    }

                    var response = await httpClient.SendAsync(request, stoppingToken);
                    if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                    {
                        continue;
                    }

                    response.EnsureSuccessStatusCode();

                    using (var stream = await response.Content.ReadAsStreamAsync(stoppingToken))
                    using (XmlReader reader = XmlReader.Create(stream))
                    {
                        SyndicationFeed feed = SyndicationFeed.Load(reader);
                        foreach (SyndicationItem item in feed.Items)
                        {
                            string author = item.Authors.Count > 0 ? item.Authors[0].Name : string.Empty;
                            string url = item.Links.Count > 0 ? item.Links[0].Uri.ToString() : string.Empty;
                            dbContext.News.Add(new News
                            {
                                Title = item.Title.Text,
                                Description = item.Summary?.Text,
                                PublishDate = item.PublishDate.DateTime,
                                Author = author,
                                Url = url,
                                Categories = string.Join(", ", item.Categories.Select(c => c.Name)),
                                Contributors = string.Join(", ", item.Contributors.Select(c => c.Name)),
                                Copyright = item.Copyright?.Text,
                                FeedSourceId = feedSource.Id
                            });
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while fetching RSS feeds.");
            }

            await Task.Delay(1_000, stoppingToken);
            //await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }

    public override void Dispose()
    {
        httpClient?.Dispose();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}
