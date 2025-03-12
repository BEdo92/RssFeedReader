using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RssFeedReader.Data;
using RssFeedReader.Models;

namespace RssFeedReader.Controllers;

[Authorize]
public class StatisticsController(FeedContext context) : BaseApiController
{
    [HttpPost("{newsId}")]
    public async Task<IActionResult> IncrementViewCount(int newsId)
    {
        var statistics = await context.Statistics.FirstOrDefaultAsync(s => s.NewsId == newsId);

        if (statistics == null)
        {
            statistics = new Statistics { NewsId = newsId, ViewCount = 1 };
            context.Statistics.Add(statistics);
        }
        else
        {
            statistics.ViewCount++;
        }

        await context.SaveChangesAsync();
        return Ok();
    }
}
