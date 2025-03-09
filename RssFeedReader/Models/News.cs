namespace RssFeedReader.Models;

public class News
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public string Url { get; set; }
    public string Author { get; set; }
    public DateTime PublishDate { get; set; }
    public int FeedSourceId { get; set; }
    public FeedSource FeedSource { get; set; }
    public string Categories { get; internal set; }
    public string Contributors { get; internal set; }
    public string? Copyright { get; internal set; }
    public string Generator { get; internal set; }
    public string? ImageUrl { get; internal set; }
    public string Language { get; internal set; }
    public DateTime LastUpdatedTime { get; internal set; }
}
