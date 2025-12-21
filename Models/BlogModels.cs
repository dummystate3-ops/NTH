namespace NovaToolsHub.Models;

/// <summary>
/// Blog post entity for content management
/// </summary>
public class BlogPost
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? MetaDescription { get; set; }
    public string? FeaturedImage { get; set; }
    public int CategoryId { get; set; }
    public string Author { get; set; } = "Admin";
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public int ViewCount { get; set; }
    public string Tags { get; set; } = string.Empty; // Comma-separated

    // Navigation property
    public BlogCategory? Category { get; set; }
}

/// <summary>
/// Blog category for organizing posts
/// </summary>
public class BlogCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
}
