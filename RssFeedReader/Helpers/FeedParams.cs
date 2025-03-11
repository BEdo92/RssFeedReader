namespace RssFeedReader.Helpers;

public class FeedParams
{
    public string? Author { get; set; }
    public string? Categories { get; set; }
    public string? FeedSource { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
