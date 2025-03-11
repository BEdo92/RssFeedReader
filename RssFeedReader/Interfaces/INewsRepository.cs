using RssFeedReader.Helpers;
using RssFeedReader.Models;

namespace RssFeedReader.Interfaces;

public interface INewsRepository
{
    IQueryable<News> GetNews(FeedParams feedParams);
}
