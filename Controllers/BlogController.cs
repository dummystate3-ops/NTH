using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using NovaToolsHub.Models;
using NovaToolsHub.Helpers;

namespace NovaToolsHub.Controllers;

/// <summary>
/// Blog controller for managing and displaying blog posts
/// </summary>
public class BlogController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BlogController> _logger;

    public BlogController(
        ApplicationDbContext context,
        IConfiguration configuration,
        ILogger<BlogController> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Blog listing page
    /// </summary>
    public async Task<IActionResult> Index(int page = 1, string? category = null)
    {
        const int pageSize = 12;
        
        var query = _context.BlogPosts
            .Include(p => p.Category)
            .Where(p => p.IsPublished);

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category!.Slug == category);
        }

        var totalPosts = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.PageTitle = "Blog - NovaTools Hub";
        ViewBag.MetaDescription = "Read articles, tutorials, and tips about calculators, conversions, and productivity tools.";
        ViewBag.CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/blog";
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);
        ViewBag.CurrentCategory = category;

        var categories = await _context.BlogCategories.ToListAsync();
        ViewBag.Categories = categories;

        return View(posts);
    }

    /// <summary>
    /// Individual blog post page
    /// </summary>
    [HttpGet("/Blog/{slug}")]
    public async Task<IActionResult> Post(string slug)
    {
        var post = await _context.BlogPosts
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

        if (post == null)
        {
            return NotFound();
        }

        // Increment view count
        post.ViewCount++;
        await _context.SaveChangesAsync();

        // Set SEO data
        ViewBag.PageTitle = $"{post.Title} - NovaTools Hub Blog";
        ViewBag.MetaDescription = post.MetaDescription ?? post.Excerpt;
        ViewBag.CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/blog/{slug}";
        ViewBag.OgImage = post.FeaturedImage ?? "/images/og-default.png";
        ViewBag.OgType = "article";

        // Generate blog post JSON-LD schema
        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://localhost:5001";
        ViewBag.JsonLdSchema = SeoHelper.GenerateBlogPostingSchema(
            post.Title,
            post.MetaDescription ?? post.Excerpt,
            $"{baseUrl}/Blog/{slug}",
            post.FeaturedImage ?? "/images/og-default.png",
            post.Author,
            post.PublishedDate ?? post.CreatedDate,
            post.PublishedDate ?? post.CreatedDate
        );

        // Breadcrumb
        ViewBag.Breadcrumbs = new List<(string Name, string Url)>
        {
            ("Home", "/"),
            ("Blog", "/Blog"),
            (post.Title, $"/Blog/{slug}")
        };

        return View(post);
    }

    /// <summary>
    /// Search blog posts
    /// </summary>
    public async Task<IActionResult> Search(string q, int page = 1)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return RedirectToAction(nameof(Index));
        }

        const int pageSize = 12;
        
        var query = _context.BlogPosts
            .Include(p => p.Category)
            .Where(p => p.IsPublished && 
                   (p.Title.Contains(q) || 
                    p.Content.Contains(q) || 
                    p.Tags.Contains(q)));

        var totalPosts = await query.CountAsync();
        var posts = await query
            .OrderByDescending(p => p.PublishedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.PageTitle = $"Search Results for '{q}' - NovaTools Hub Blog";
        ViewBag.MetaDescription = $"Search results for '{q}' in NovaTools Hub blog.";
        ViewBag.SearchQuery = q;
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalPosts / (double)pageSize);

        return View("Index", posts);
    }

    /// <summary>
    /// RSS feed for blog posts
    /// </summary>
    [HttpGet("/Blog/RSS")]
    public async Task<IActionResult> RSS()
    {
        var posts = await _context.BlogPosts
            .Include(p => p.Category)
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .Take(20)
            .ToListAsync();

        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://NovaToolsHub.com";
        
        var rss = new System.Text.StringBuilder();
        rss.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
        rss.AppendLine("<rss version=\"2.0\" xmlns:atom=\"http://www.w3.org/2005/Atom\">");
        rss.AppendLine("<channel>");
        rss.AppendLine("<title>NovaToolsHub Blog</title>");
        rss.AppendLine("<description>Latest articles, tips, and insights from NovaToolsHub</description>");
        rss.AppendLine($"<link>{baseUrl}/Blog</link>");
        rss.AppendLine($"<atom:link href=\"{baseUrl}/Blog/RSS\" rel=\"self\" type=\"application/rss+xml\" />");
        rss.AppendLine($"<lastBuildDate>{DateTime.UtcNow:R}</lastBuildDate>");

        foreach (var post in posts)
        {
            rss.AppendLine("<item>");
            rss.AppendLine($"<title><![CDATA[{post.Title}]]></title>");
            rss.AppendLine($"<description><![CDATA[{post.Excerpt}]]></description>");
            rss.AppendLine($"<link>{baseUrl}/Blog/{post.Slug}</link>");
            rss.AppendLine($"<guid isPermaLink=\"true\">{baseUrl}/Blog/{post.Slug}</guid>");
            rss.AppendLine($"<pubDate>{(post.PublishedDate ?? post.CreatedDate):R}</pubDate>");
            rss.AppendLine($"<author>{post.Author}</author>");
            rss.AppendLine($"<category>{post.Category?.Name ?? "General"}</category>");
            rss.AppendLine("</item>");
        }

        rss.AppendLine("</channel>");
        rss.AppendLine("</rss>");

        return Content(rss.ToString(), "application/rss+xml");
    }
}
