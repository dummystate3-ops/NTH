using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using NovaToolsHub.Filters;
using NovaToolsHub.Models;

namespace NovaToolsHub.Areas.Admin.Controllers;

[Area("Admin")]
[AdminAuth]
public class BlogController : Controller
{
    private readonly ApplicationDbContext _context;

    public BlogController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List all blog posts (published and drafts)
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var posts = await _context.BlogPosts
            .Include(p => p.Category)
            .OrderByDescending(p => p.CreatedDate)
            .ToListAsync();

        return View(posts);
    }

    /// <summary>
    /// Create new blog post form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.BlogCategories.ToListAsync();
        return View(new BlogPost());
    }

    /// <summary>
    /// Save new blog post
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlogPost post)
    {
        // Remove navigation property from ModelState validation
        ModelState.Remove("Category");
        ModelState.Remove("Excerpt"); // Excerpt is optional/auto-generated
        
        if (string.IsNullOrWhiteSpace(post.Title))
        {
            ModelState.AddModelError("Title", "Title is required");
        }

        if (post.CategoryId == 0)
        {
            ModelState.AddModelError("CategoryId", "Category is required");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Generate slug from title if not provided
                if (string.IsNullOrWhiteSpace(post.Slug))
                {
                    post.Slug = GenerateSlug(post.Title);
                }

                // Ensure slug is unique
                var existingSlug = await _context.BlogPosts.AnyAsync(p => p.Slug == post.Slug);
                if (existingSlug)
                {
                    post.Slug = $"{post.Slug}-{DateTime.UtcNow.Ticks}";
                }

                // Generate excerpt if not provided or empty
                if (string.IsNullOrWhiteSpace(post.Excerpt) && !string.IsNullOrWhiteSpace(post.Content))
                {
                    var plainText = System.Text.RegularExpressions.Regex.Replace(post.Content, "<.*?>", "");
                    post.Excerpt = plainText.Length > 200 ? plainText.Substring(0, 200) + "..." : plainText;
                }
                
                // Ensure excerpt has a value
                if (string.IsNullOrWhiteSpace(post.Excerpt))
                {
                    post.Excerpt = post.Title;
                }

                post.CreatedDate = DateTime.UtcNow;
                post.Author = HttpContext.Session.GetString("AdminUsername") ?? "Admin";

                if (post.IsPublished && post.PublishedDate == null)
                {
                    post.PublishedDate = DateTime.UtcNow;
                }

                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Blog post created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating post: {ex.Message}");
            }
        }

        ViewBag.Categories = await _context.BlogCategories.ToListAsync();
        return View(post);
    }

    /// <summary>
    /// Edit blog post form
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        ViewBag.Categories = await _context.BlogCategories.ToListAsync();
        return View(post);
    }

    /// <summary>
    /// Update blog post
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BlogPost post)
    {
        if (id != post.Id)
        {
            return NotFound();
        }

        // Remove navigation property from ModelState validation
        ModelState.Remove("Category");

        if (ModelState.IsValid)
        {
            try
            {
                var existingPost = await _context.BlogPosts.FindAsync(id);
                if (existingPost == null)
                {
                    return NotFound();
                }

                existingPost.Title = post.Title;
                existingPost.Slug = post.Slug;
                existingPost.Content = post.Content;
                existingPost.Excerpt = post.Excerpt;
                existingPost.MetaDescription = post.MetaDescription;
                existingPost.FeaturedImage = post.FeaturedImage;
                existingPost.CategoryId = post.CategoryId;
                existingPost.Tags = post.Tags;
                existingPost.IsPublished = post.IsPublished;

                if (post.IsPublished && existingPost.PublishedDate == null)
                {
                    existingPost.PublishedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Blog post updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.BlogPosts.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        ViewBag.Categories = await _context.BlogCategories.ToListAsync();
        return View(post);
    }

    /// <summary>
    /// Delete blog post
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        _context.BlogPosts.Remove(post);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Blog post deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Toggle publish status
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> TogglePublish(int id)
    {
        var post = await _context.BlogPosts.FindAsync(id);
        if (post == null)
        {
            return NotFound();
        }

        post.IsPublished = !post.IsPublished;
        if (post.IsPublished && post.PublishedDate == null)
        {
            post.PublishedDate = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Generate URL-friendly slug from title
    /// </summary>
    private string GenerateSlug(string title)
    {
        var slug = title.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and");

        // Remove invalid characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }
}
