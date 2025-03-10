using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;

namespace RssFeedReader.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController(FeedContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetNews()
    {
        return Ok(await context.News.ToListAsync());
    }

    [HttpGet("filter")]
    public async Task<IActionResult> GetNews(int page = 1, int pageSize = 10)
    {
        return Ok(await context.News
            .OrderByDescending(n => n.PublishDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync());
    }
}
