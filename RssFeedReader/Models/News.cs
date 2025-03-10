namespace RssFeedReader.Models;

public class News
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Content { get; set; }
    public string Url { get; set; }
    public string? Author { get; set; }
    public DateTime PublishDate { get; set; }
    public int FeedSourceId { get; set; }
    public FeedSource FeedSource { get; set; }
    public string? Categories { get; set; }
    public string? Contributors { get; set; }
    public string? Copyright { get; set; }
    public string? Generator { get; set; }
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public DateTime? LastUpdatedTime { get; set; }
}
