using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RssFeedReader.DTOs;
using RssFeedReader.Helpers;
using RssFeedReader.Interfaces;
using RssFeedReader.Models;

namespace RssFeedReader.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(INewsRepository repository, IMapper mapper) : ControllerBase
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
        IQueryable<News> news = repository.GetNews(new FeedParams
        {
            Author = author,
            Categories = category,
            FeedSource = feedSource,
            FromDate = fromDate,
            ToDate = toDate
        });

        var pagedNews = await PagedList<News>.CreateAsync(news, page, pageSize);
        var newsDTOs = pagedNews.Items.Select(mapper.Map<NewsDTO>).ToList();
        var pagedNewsDTOs = new PagedList<NewsDTO>(newsDTOs, pagedNews.TotalCount, page, pageSize);
        return Ok(pagedNewsDTOs);
    }
}
