using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;
using RssFeedReader.DTOs;
using RssFeedReader.Helpers;
using RssFeedReader.Models;

namespace RssFeedReader.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(FeedContext context, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNews(
        int page = 1,
        int pageSize = 10,
        string? author = null,
        string? category = null,
        string? feedSource = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)

    {
        var news = context.News.OrderByDescending(x => x.PublishDate)
            .Include(x => x.FeedSource)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrEmpty(author))
        {
            news = news.Where(n => n.Author == author);
        }

        if (!string.IsNullOrEmpty(category))
        {
            news = news.Where(n => n.Categories != null && n.Categories.Contains(category));
        }

        if (!string.IsNullOrEmpty(feedSource))
        {
            news = news.Where(n => n.FeedSource != null && n.FeedSource.Name.Equals(feedSource));
        }

        if (fromDate.HasValue)
        {
            news = news.Where(n => n.PublishDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            news = news.Where(n => n.PublishDate <= toDate.Value);
        }

        var pagedNews = await PagedList<News>.CreateAsync(news, page, pageSize);
        var newsDTOs = pagedNews.Items.Select(mapper.Map<NewsDTO>).ToList();
        var pagedNewsDTOs = new PagedList<NewsDTO>(newsDTOs, pagedNews.TotalCount, page, pageSize);
        return Ok(pagedNewsDTOs);
    }
}
