using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using NovaToolsHub.Filters;

namespace NovaToolsHub.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuth]
[Route("Admin/Analytics")]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AnalyticsController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var model = new AnalyticsViewModel
        {
            // Blog Statistics
            TotalBlogPosts = await _context.BlogPosts.CountAsync(),
            PublishedPosts = await _context.BlogPosts.CountAsync(p => p.IsPublished),
            DraftPosts = await _context.BlogPosts.CountAsync(p => !p.IsPublished),
            TotalViews = await _context.BlogPosts.SumAsync(p => p.ViewCount),

            // Recent Blog Posts
            RecentPosts = await _context.BlogPosts
                .OrderByDescending(p => p.PublishedDate)
                .Take(10)
                .Select(p => new BlogPostSummary
                {
                    Title = p.Title,
                    Slug = p.Slug,
                    PublishedDate = p.PublishedDate,
                    ViewCount = p.ViewCount,
                    IsPublished = p.IsPublished
                })
                .ToListAsync(),

            // Top Posts by Views
            TopPosts = await _context.BlogPosts
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.ViewCount)
                .Take(10)
                .Select(p => new BlogPostSummary
                {
                    Title = p.Title,
                    Slug = p.Slug,
                    PublishedDate = p.PublishedDate,
                    ViewCount = p.ViewCount,
                    IsPublished = p.IsPublished
                })
                .ToListAsync(),

            // Analytics Configuration
            GoogleAnalyticsConfigured = !string.IsNullOrEmpty(_configuration["GoogleAnalytics:MeasurementId"]),
            AdSenseConfigured = !string.IsNullOrEmpty(_configuration["AdSense:PublisherId"]),
            GoogleAnalyticsId = _configuration["GoogleAnalytics:MeasurementId"],
            AdSensePublisherId = _configuration["AdSense:PublisherId"]
        };

        return View(model);
    }

    [HttpGet("Export")]
    public async Task<IActionResult> ExportData(string format = "csv")
    {
        var posts = await _context.BlogPosts
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();

        if (format.ToLower() == "csv")
        {
            var csv = "Title,Slug,Author,Published Date,View Count,Status\n";
            foreach (var post in posts)
            {
                csv += $"\"{post.Title}\",\"{post.Slug}\",\"{post.Author}\",\"{post.PublishedDate:yyyy-MM-dd}\",{post.ViewCount},\"{(post.IsPublished ? "Published" : "Draft")}\"\n";
            }

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"blog-analytics-{DateTime.Now:yyyyMMdd}.csv");
        }

        return BadRequest("Unsupported format");
    }
}

public class AnalyticsViewModel
{
    public int TotalBlogPosts { get; set; }
    public int PublishedPosts { get; set; }
    public int DraftPosts { get; set; }
    public int TotalViews { get; set; }
    public List<BlogPostSummary> RecentPosts { get; set; } = new();
    public List<BlogPostSummary> TopPosts { get; set; } = new();
    public bool GoogleAnalyticsConfigured { get; set; }
    public bool AdSenseConfigured { get; set; }
    public string? GoogleAnalyticsId { get; set; }
    public string? AdSensePublisherId { get; set; }
}

public class BlogPostSummary
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public DateTime? PublishedDate { get; set; }
    public int ViewCount { get; set; }
    public bool IsPublished { get; set; }
}
