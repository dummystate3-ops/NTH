using System.Text.Json;

namespace NovaToolsHub.Helpers;

/// <summary>
/// Helper methods for generating SEO metadata and JSON-LD schemas
/// </summary>
public static class SeoHelper
{
    /// <summary>
    /// Generate WebSite schema
    /// </summary>
    public static string GenerateWebSiteSchema(string siteName, string url, string searchUrl)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "WebSite",
            name = siteName,
            url = url,
            potentialAction = new
            {
                type = "SearchAction",
                target = new
                {
                    type = "EntryPoint",
                    urlTemplate = $"{searchUrl}?q={{search_term_string}}"
                },
                queryInput = "required name=search_term_string"
            }
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate WebPage schema
    /// </summary>
    public static string GenerateWebPageSchema(string name, string description, string url, string imageUrl)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "WebPage",
            name = name,
            description = description,
            url = url,
            image = imageUrl
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate SoftwareApplication schema for tools
    /// </summary>
    public static string GenerateSoftwareApplicationSchema(string name, string description, string url, string category)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "SoftwareApplication",
            name = name,
            description = description,
            url = url,
            applicationCategory = category,
            offers = new
            {
                type = "Offer",
                price = "0",
                priceCurrency = "USD"
            },
            operatingSystem = "Any",
            browserRequirements = "Requires JavaScript. Requires HTML5."
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate FAQPage schema
    /// </summary>
    public static string GenerateFaqPageSchema(List<(string Question, string Answer)> faqs)
    {
        var mainEntity = faqs.Select(faq => new
        {
            type = "Question",
            name = faq.Question,
            acceptedAnswer = new
            {
                type = "Answer",
                text = faq.Answer
            }
        }).ToList();

        var schema = new
        {
            context = "https://schema.org",
            type = "FAQPage",
            mainEntity
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate BlogPosting schema
    /// </summary>
    public static string GenerateBlogPostingSchema(string headline, string description, string url, 
        string imageUrl, string authorName, DateTime publishedDate, DateTime modifiedDate)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "BlogPosting",
            headline = headline,
            description = description,
            url = url,
            image = imageUrl,
            author = new
            {
                type = "Person",
                name = authorName
            },
            publisher = new
            {
                type = "Organization",
                name = "NovaTools Hub",
                logo = new
                {
                    type = "ImageObject",
                    url = "/images/logo.png"
                }
            },
            datePublished = publishedDate.ToString("yyyy-MM-dd"),
            dateModified = modifiedDate.ToString("yyyy-MM-dd")
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate FinancialService schema for financial calculators
    /// </summary>
    public static string GenerateFinancialServiceSchema(string name, string description, string url)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "FinancialService",
            name = name,
            description = description,
            url = url,
            serviceType = "Financial Calculator"
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate LearningResource schema for educational tools
    /// </summary>
    public static string GenerateLearningResourceSchema(string name, string description, string url, string educationalLevel)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "LearningResource",
            name = name,
            description = description,
            url = url,
            educationalLevel = educationalLevel,
            learningResourceType = "Tool"
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate BreadcrumbList schema
    /// </summary>
    public static string GenerateBreadcrumbSchema(List<(string Name, string Url)> breadcrumbs)
    {
        var itemListElement = breadcrumbs.Select((crumb, index) => new
        {
            type = "ListItem",
            position = index + 1,
            name = crumb.Name,
            item = crumb.Url
        }).ToList();

        var schema = new
        {
            context = "https://schema.org",
            type = "BreadcrumbList",
            itemListElement
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }

    /// <summary>
    /// Generate Organization schema
    /// </summary>
    public static string GenerateOrganizationSchema(string name, string url, string logoUrl, 
        List<string> socialMediaUrls)
    {
        var schema = new
        {
            context = "https://schema.org",
            type = "Organization",
            name = name,
            url = url,
            logo = logoUrl,
            sameAs = socialMediaUrls
        };

        return JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });
    }
}
