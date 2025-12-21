using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using System.Text;
using System.Xml.Linq;

namespace NovaToolsHub.Controllers;

/// <summary>
/// Controller for generating SEO-related files (sitemap.xml, robots.txt)
/// </summary>
public class SitemapController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public SitemapController(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Generates sitemap.xml for search engine crawlers
    /// </summary>
    [Route("sitemap.xml")]
    [HttpGet]
    public async Task<IActionResult> SitemapXml()
    {
        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://NovaToolsHub.com";
        
        XNamespace sitemap = "http://www.sitemaps.org/schemas/sitemap/0.9";
        
        var urlset = new XElement(sitemap + "urlset");

        // Homepage
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/", "1.0", "daily"));

        // Static Pages
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/home/about", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools", "0.9", "weekly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/blog", "0.8", "daily"));

        // Converter & Utility Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/unitconverter", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/currencyconverter", "0.8", "daily"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/bmicalculator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/agecalculator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/datecalculator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/passwordgenerator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/qrcodegenerator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/colorpalettegenerator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/jsonformatter", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/regextester", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/loremipsum", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/favicongenerator", "0.7", "monthly"));

        // Image Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/image/advancedresizer", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/tools/image/backgroundremover", "0.8", "monthly"));

        // Math Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/equationsolver", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/permutationcombination", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/cryptography", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/expressionevaluator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/indicatorcodes", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/problemsolver", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/math/graphplotter", "0.8", "monthly"));

        // Productivity Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/worldtime", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/timezoneconverter", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/stopwatch", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/notes", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/texttospeech", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/timetracker", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/todolist", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/pomodorotimer", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/productivity/kanbanboard", "0.7", "monthly"));

        // Business Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/business/profitmargin", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/business/roianalysis", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/business/unbilledhours", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/business/savingscomparison", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/business/loancalculator", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/business/automationplanner", "0.7", "monthly"));

        // Academic Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/academic/quizbuilder", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/academic/flashcards", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/academic/grammarhelper", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/academic/formulareference", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/academic/whiteboard", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/academic/mindmap", "0.7", "monthly"));

        // Trending Tools
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/trending/aiwritingassistant", "0.8", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/trending/ratecomparison", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/trending/pollbuilder", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/trending/encryptiontool", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/trending/memegenerator", "0.7", "monthly"));
        urlset.Add(CreateUrlElement(sitemap, baseUrl + "/trending/recipegenerator", "0.7", "monthly"));

        // Dynamic Blog Posts
        var blogPosts = await _context.BlogPosts
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .ToListAsync();

        foreach (var post in blogPosts)
        {
            urlset.Add(CreateUrlElement(
                sitemap,
                baseUrl + "/blog/post/" + post.Slug,
                "0.6",
                "monthly",
                post.PublishedDate
            ));
        }

        var document = new XDocument(
            new XDeclaration("1.0", "utf-8", "yes"),
            urlset
        );

        return Content(document.ToString(), "application/xml", Encoding.UTF8);
    }

    /// <summary>
    /// Generates robots.txt for search engine crawlers
    /// </summary>
    [Route("robots.txt")]
    [HttpGet]
    public IActionResult RobotsTxt()
    {
        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://NovaToolsHub.com";

        var sb = new StringBuilder();
        sb.AppendLine("User-agent: *");
        sb.AppendLine("Allow: /");
        sb.AppendLine();
        sb.AppendLine("# Disallow admin areas");
        sb.AppendLine("Disallow: /Admin/");
        sb.AppendLine("Disallow: /Areas/Admin/");
        sb.AppendLine();
        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");

        return Content(sb.ToString(), "text/plain", Encoding.UTF8);
    }

    /// <summary>
    /// Helper method to create URL elements for sitemap
    /// </summary>
    private XElement CreateUrlElement(XNamespace ns, string loc, string priority, string changefreq, DateTime? lastmod = null)
    {
        var urlElement = new XElement(ns + "url",
            new XElement(ns + "loc", loc),
            new XElement(ns + "changefreq", changefreq),
            new XElement(ns + "priority", priority)
        );

        if (lastmod.HasValue)
        {
            urlElement.Add(new XElement(ns + "lastmod", lastmod.Value.ToString("yyyy-MM-dd")));
        }

        return urlElement;
    }
}
