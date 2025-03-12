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

        if (!string.IsNullOrEmpty(feedParams.Title))
        {
            news = news.Where(n => n.Title != null && n.Title.Contains(feedParams.Title));
        }

        if (!string.IsNullOrEmpty(feedParams.FeedSource))
        {
            news = news.Where(n => n.FeedSource != null && n.FeedSource.Name.Equals(feedParams.FeedSource));
        }

        if (feedParams.FromDate.HasValue)
        {
            news = news.Where(n => n.PublishDate >= feedParams.FromDate.Value);
        }

        if (feedParams.ToDate.HasValue)
        {
            news = news.Where(n => n.PublishDate <= feedParams.ToDate.Value);
        }

        return news;
    }
}
