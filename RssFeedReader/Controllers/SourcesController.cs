using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;

namespace RssFeedReader.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SourcesController(FeedContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetSources()
    {
        var sources = await context.FeedSources.Select(x => x.Name).ToListAsync();
        return Ok(sources);
    }
}
