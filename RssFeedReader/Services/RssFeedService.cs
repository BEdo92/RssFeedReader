﻿using Microsoft.EntityFrameworkCore;
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
    private const int FETCHING_INTERVAL = 5;

    public RssFeedService(IServiceScopeFactory scopeFactory, ILogger<RssFeedService> logger)
    {
        this.scopeFactory = scopeFactory;
        this.logger = logger;

        // TODO: Remove this line in production!!
        HttpClientHandler handler = new()
        {
            //ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        };
        httpClient = new HttpClient(handler);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await InitialLoad(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                logger.LogInformation("Fetching RSS feeds...");

                using IServiceScope scope = scopeFactory.CreateScope();
                FeedContext dbContext = scope.ServiceProvider.GetRequiredService<FeedContext>();
                List<FeedSource> feedSources = await dbContext.FeedSources.ToListAsync(stoppingToken);

                foreach (FeedSource? feedSource in feedSources)
                {
                    HttpResponseMessage response;
                    try
                    {
                        response = await httpClient.GetAsync(feedSource.Url, stoppingToken);
                        response.EnsureSuccessStatusCode();

                        using Stream stream = await response.Content.ReadAsStreamAsync(stoppingToken);
                        await ProcessFeedAsync(dbContext, feedSource, stream, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Error while sending the request for feed source: {feedSource}. {e}", feedSource, e);
                    }
                }
            }
            catch (Exception ex)
            {
                // NOTE: Some RSS feeds fail sporadically. They overall work, but sometimes fail for some reason.
                logger.LogError(ex, "Error occurred while fetching and processing RSS feeds at {hour}:{minute}.", DateTime.Now.Hour, DateTime.Now.Minute);
            }

            await Task.Delay(TimeSpan.FromMinutes(FETCHING_INTERVAL), stoppingToken);
        }
    }

    private async Task InitialLoad(CancellationToken stoppingToken)
    {
        try
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            FeedContext dbContext = scope.ServiceProvider.GetRequiredService<FeedContext>();
            List<FeedSource> feedSources = await dbContext.FeedSources.ToListAsync(stoppingToken);

            foreach (FeedSource? feedSource in feedSources)
            {
                HttpResponseMessage response;
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, feedSource.Url);
                    // NOTE: This is not the best way to handle the If-Modified-Since header, as RSS Servers may not support
                    // it or they may use the date of the modification of the XML file, not the date of the last news news.
                    request.Headers.IfModifiedSince = DateTime.Now.AddDays(-7).ToUniversalTime();
                    response = await httpClient.SendAsync(request, stoppingToken);

                    if (response.StatusCode == System.Net.HttpStatusCode.NotModified)
                    {
                        logger.LogInformation("No new feeds found for {FeedSourceName}.", feedSource.Name);
                        continue;
                    }

                    response.EnsureSuccessStatusCode();

                    using Stream stream = await response.Content.ReadAsStreamAsync(stoppingToken);
                    await ProcessFeedAsync(dbContext, feedSource, stream, stoppingToken);
                }
                catch (Exception)
                {
                    logger.LogError("Error occurred while processing feed: {feedSource} durint initial load.", feedSource);
                }
            }
        }
        catch (Exception)
        {
            logger.LogError("Error occurred while fetching and processing RSS feeds during the initial load.");
        }
    }

    private async Task ProcessFeedAsync(FeedContext dbContext, FeedSource feedSource, Stream stream, CancellationToken stoppingToken)
    {
        using (XmlReader reader = XmlReader.Create(stream))
        {
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            foreach (SyndicationItem feedItem in feed.Items)
            {
                string guid = feedItem.Id;

                if (await IsArticleAlreadyExistsAsync(dbContext, feedItem, guid, stoppingToken))
                {
                    continue;
                }

                string author = feedItem.Authors.Count > 0 ? feedItem.Authors[0].Name : string.Empty;
                string url = feedItem.Links.Count > 0 ? feedItem.Links[0].Uri.ToString() : string.Empty;
                dbContext.News.Add(new News
                {
                    Title = feedItem.Title.Text,
                    Guid = guid,
                    Description = feedItem.Summary?.Text,
                    PublishDate = feedItem.PublishDate.DateTime,
                    Author = author,
                    Url = url,
                    Categories = string.Join(", ", feedItem.Categories.Select(c => c.Name)),
                    Contributors = string.Join(", ", feedItem.Contributors.Select(c => c.Name)),
                    Copyright = feedItem.Copyright?.Text,
                    FeedSourceId = feedSource.Id,
                    ImageUrl = feedItem.Links.FirstOrDefault(l => l.MediaType == "image/jpeg" || l.MediaType == "image/png")?.Uri.ToString(),
                });

                logger.LogInformation("New article added: {Title}", feedItem.Title.Text);
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);
        logger.LogInformation("Feed {FeedSourceName} processed successfully.", feedSource.Name);
    }

    private async Task<bool> IsArticleAlreadyExistsAsync(FeedContext dbContext, SyndicationItem feedItem, string guid, CancellationToken stoppingToken)
    {
        if (!string.IsNullOrEmpty(guid))
        {
            if (await dbContext.News.AnyAsync(n => n.Guid == guid, stoppingToken))
            {
                logger.LogDebug("The article with guid {guid} already exists in the database.", guid);
                return true;
            }
        }
        else
        {
            if (await dbContext.News.AnyAsync(n => n.Title == feedItem.Title.Text && n.PublishDate == feedItem.PublishDate.DateTime, stoppingToken))
            {
                logger.LogDebug("The article with title {title} and publish date {publishdate} exists in the database.", feedItem.Title.Text, feedItem.PublishDate.DateTime);
                return true;
            }
        }

        return false;
    }

    public override void Dispose()
    {
        httpClient?.Dispose();
        GC.SuppressFinalize(this);
        base.Dispose();
    }
}
