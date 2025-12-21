using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Helpers;

namespace NovaToolsHub.Controllers;

/// <summary>
/// Main home controller for NovaTools Hub
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;

    public HomeController(ILogger<HomeController> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Home page with tool categories and featured tools
    /// </summary>
    public IActionResult Index()
    {
        var model = new HomeViewModel
        {
            PageTitle = "NovaTools Hub - Calculators & Tools for Everyone",
            MetaDescription = "Access a comprehensive suite of calculators and tools for business, education, conversions, and productivity. Free, fast, and easy to use.",
            CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/",
            OgType = "website",
            ToolCategories = GetToolCategories(),
            FeaturedTools = GetFeaturedTools()
        };

        // Generate JSON-LD schema for the homepage
        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://localhost:5001";
        ViewBag.JsonLdSchema = SeoHelper.GenerateWebSiteSchema(
            "NovaTools Hub",
            baseUrl,
            $"{baseUrl}/search"
        );

        SetSeoData(model);
        return View(model);
    }

    /// <summary>
    /// About page
    /// </summary>
    public IActionResult About()
    {
        ViewBag.PageTitle = "About Us - NovaTools Hub";
        ViewBag.MetaDescription = "Learn more about NovaTools Hub and our mission to provide accessible, powerful tools for everyone.";
        ViewBag.CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/home/about";
        
        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://localhost:5001";
        ViewBag.JsonLdSchema = SeoHelper.GenerateOrganizationSchema(
            "NovaTools Hub",
            baseUrl,
            $"{baseUrl}/images/logo.png",
            new List<string> { "https://twitter.com/NovaToolsHub", "https://linkedin.com/company/NovaToolsHub" }
        );
        
        return View();
    }

    /// <summary>
    /// Contact page
    /// </summary>
    public IActionResult Contact()
    {
        ViewBag.PageTitle = "Contact Us - NovaTools Hub";
        ViewBag.MetaDescription = "Get in touch with the NovaTools Hub team. We'd love to hear from you!";
        ViewBag.CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/home/contact";
        
        return View();
    }

    /// <summary>
    /// Privacy policy page
    /// </summary>
    public IActionResult Privacy()
    {
        ViewBag.PageTitle = "Privacy Policy - NovaTools Hub";
        ViewBag.MetaDescription = "Read our privacy policy to understand how we collect, use, and protect your data.";
        ViewBag.CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/home/privacy";
        
        return View();
    }

    /// <summary>
    /// Terms of service page
    /// </summary>
    public IActionResult Terms()
    {
        ViewBag.PageTitle = "Terms of Service - NovaTools Hub";
        ViewBag.MetaDescription = "Read our terms of service to understand the rules and guidelines for using NovaTools Hub.";
        ViewBag.CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/home/terms";
        
        return View();
    }

    /// <summary>
    /// All tools page showing complete tool list
    /// </summary>
    public IActionResult AllTools()
    {
        var model = new BasePageViewModel
        {
            PageTitle = "All Tools - NovaTools Hub",
            MetaDescription = "Browse our complete collection of 48+ calculators and tools for business, education, productivity, conversions, and more.",
            CanonicalUrl = $"{_configuration["SiteSettings:BaseUrl"]}/tools"
        };

        var baseUrl = _configuration["SiteSettings:BaseUrl"] ?? "https://localhost:5001";
        ViewBag.JsonLdSchema = SeoHelper.GenerateWebPageSchema(
            "All Tools - NovaTools Hub",
            "Browse all calculators and tools including converters, math solvers, business calculators, and productivity tools.",
            $"{baseUrl}/tools",
            $"{baseUrl}/images/og-default.png"
        );

        SetSeoData(model);
        return View(model);
    }

    /// <summary>
    /// Error page
    /// </summary>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        ViewBag.PageTitle = "Error - NovaTools Hub";
        return View();
    }

    // Helper methods

    private void SetSeoData(BasePageViewModel model)
    {
        ViewBag.PageTitle = model.PageTitle;
        ViewBag.MetaDescription = model.MetaDescription;
        ViewBag.CanonicalUrl = model.CanonicalUrl;
        ViewBag.OgImage = model.OgImage;
        ViewBag.OgType = model.OgType;
    }

    private List<ToolCategory> GetToolCategories()
    {
        return new List<ToolCategory>
        {
            new ToolCategory
            {
                Name = "Converters",
                Slug = "converters",
                Description = "Convert units, currencies, and measurements instantly",
                Icon = "📊",
                ToolCount = 7
            },
            new ToolCategory
            {
                Name = "Math & Education",
                Slug = "math",
                Description = "Advanced calculators for mathematical operations",
                Icon = "🔢",
                ToolCount = 7
            },
            new ToolCategory
            {
                Name = "Productivity",
                Slug = "productivity",
                Description = "Tools to boost your productivity and efficiency",
                Icon = "⚡",
                ToolCount = 9
            },
            new ToolCategory
            {
                Name = "Business & Finance",
                Slug = "business",
                Description = "Financial calculators and business planning tools",
                Icon = "💼",
                ToolCount = 6
            },
            new ToolCategory
            {
                Name = "Academic & Teacher Tools",
                Slug = "academic",
                Description = "Educational tools for students and teachers",
                Icon = "🎓",
                ToolCount = 7
            },
            new ToolCategory
            {
                Name = "Trending & AI Tools",
                Slug = "trending",
                Description = "AI-powered tools and trending utilities",
                Icon = "🔥",
                ToolCount = 6
            }
        };
    }

    private List<FeaturedTool> GetFeaturedTools()
    {
        return new List<FeaturedTool>
        {
            new FeaturedTool
            {
                Name = "Advance Image Tool",
                Description = "Crop, resize, compress, watermark, and batch download images",
                Url = "/Tools/Image/AdvancedResizer",
                Icon = "[IMG]",
                Category = "Productivity"
            },
            new FeaturedTool
            {
                Name = "Unit Converter",
                Description = "Convert between length, weight, temperature, and more",
                Url = "/Tools/UnitConverter",
                Icon = "📏",
                Category = "Converters"
            },
            new FeaturedTool
            {
                Name = "Currency Converter",
                Description = "Convert between major world currencies",
                Url = "/Tools/CurrencyConverter",
                Icon = "💱",
                Category = "Converters"
            },
            new FeaturedTool
            {
                Name = "Equation Solver",
                Description = "Solve linear, quadratic, and cubic equations with steps",
                Url = "/Math/EquationSolver",
                Icon = "🧮",
                Category = "Math"
            },
            new FeaturedTool
            {
                Name = "Graph Plotter",
                Description = "Visualize mathematical functions interactively",
                Url = "/Math/GraphPlotter",
                Icon = "📈",
                Category = "Math"
            },
            new FeaturedTool
            {
                Name = "BMI Calculator",
                Description = "Calculate your body mass index and health metrics",
                Url = "/Tools/BmiCalculator",
                Icon = "⚖️",
                Category = "Health"
            },
            new FeaturedTool
            {
                Name = "Password Generator",
                Description = "Create strong, secure passwords instantly",
                Url = "/Tools/PasswordGenerator",
                Icon = "🔑",
                Category = "Productivity"
            },
            new FeaturedTool
            {
                Name = "Pomodoro Timer",
                Description = "Boost focus with the Pomodoro Technique timer",
                Url = "/Productivity/PomodoroTimer",
                Icon = "⏱️",
                Category = "Productivity"
            },
            new FeaturedTool
            {
                Name = "Expression Evaluator",
                Description = "Evaluate complex mathematical expressions safely",
                Url = "/Math/ExpressionEvaluator",
                Icon = "➗",
                Category = "Math"
            },
            new FeaturedTool
            {
                Name = "To-Do List",
                Description = "Organize and prioritize your daily tasks",
                Url = "/Productivity/TodoList",
                Icon = "✅",
                Category = "Productivity"
            },
            new FeaturedTool
            {
                Name = "QR Code Generator",
                Description = "Generate QR codes for text, URLs, and more",
                Url = "/Tools/QrCodeGenerator",
                Icon = "📱",
                Category = "Productivity"
            }
        };
    }
}
