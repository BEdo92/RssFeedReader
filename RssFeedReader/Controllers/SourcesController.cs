using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;

namespace RssFeedReader.Controllers;

[Authorize]
public class SourcesController(FeedContext context) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> GetSources()
    {
        var sources = await context.FeedSources.Select(x => x.Name).ToListAsync();
        return Ok(sources);
    }
}
