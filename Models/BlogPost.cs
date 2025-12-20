namespace InteraktifKredi.Models;

public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public int ReadCount { get; set; }
    public bool IsFeatured { get; set; }
    public string Category { get; set; } = string.Empty;
}

