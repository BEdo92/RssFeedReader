using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;
using RssFeedReader.Helpers;
using RssFeedReader.Interfaces;
using RssFeedReader.Models;

namespace RssFeedReader.Repositories;

public class NewsRepository(FeedContext context) : INewsRepository
{
    public IQueryable<News> GetNews(FeedParams feedParams)
    {
        var news = context.News.OrderByDescending(x => x.PublishDate)
                    .Include(x => x.FeedSource)
                    .AsNoTracking()
                    .AsQueryable();

        if (!string.IsNullOrEmpty(feedParams.Author))
        {
            news = news.Where(n => n.Author == feedParams.Author);
        }

        if (!string.IsNullOrEmpty(feedParams.Categories))
        {
            news = news.Where(n => n.Categories != null && n.Categories.Contains(feedParams.Categories));
        }

        if (!string.IsNullOrEmpty(feedParams.FeedSource))
        {
            news = news.Where(n => n.FeedSource != null && n.FeedSource.Name.Equals(feedParams.FeedSource));
        }

        if (feedParams.FromDate.HasValue)
        {
            news = news.Where(n => n.PublishDate >= feedParams.FromDate.Value);
        }

        if (feedParams.FromDate.HasValue)
        {
            news = news.Where(n => n.PublishDate <= feedParams.FromDate.Value);
        }

        return news;
    }
}
