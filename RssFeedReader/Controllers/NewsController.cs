using Microsoft.AspNetCore.Mvc;
using RssFeedReader.Data;
using RssFeedReader.Helpers;
using RssFeedReader.Models;

namespace RssFeedReader.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(FeedContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNews(int page = 1, int pageSize = 10)
    {
        var news = context.News.AsQueryable();
        var pagedNews = await PagedList<News>.CreateAsync(news, page, pageSize);
        return Ok(pagedNews);
    }
}
