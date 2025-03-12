namespace RssFeedReader.Models;

public class Statistics
{
    public int Id { get; set; }
    public int NewsId { get; set; }
    public News News { get; set; }
    public int ViewCount { get; set; }
}
