namespace RssFeedReader.Helpers;

public class FeedParams
{
    public string? Url { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Author { get; set; }
    public string? Categories { get; set; }
    public string? Contributors { get; set; }
    public string? Copyright { get; set; }
    public string? Generator { get; set; }
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public DateTime? LastUpdatedTime { get; set; }
}
