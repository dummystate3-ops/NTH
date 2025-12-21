using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NovaToolsHub.Data;
using NovaToolsHub.Services;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddMemoryCache();

// Configure routing options for SEO-friendly lowercase URLs
builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
    options.AppendTrailingSlash = false;
});

// Configure Entity Framework Core with SQL Server
// Note: Update connection string in appsettings.json for production
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register application services
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<IImageProcessingService, ImageProcessingService>();
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<IMemeService, MemeService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IRateComparisonService, RateComparisonService>();
builder.Services.AddSingleton<ITempBatchStorage, TempBatchStorage>();
builder.Services.Configure<BackgroundRemovalSettings>(builder.Configuration.GetSection("BackgroundRemoval")); // Phase 3
builder.Services.AddSingleton<IBackgroundRemovalService, BackgroundRemovalService>();
builder.Services.AddSingleton<ITempResultStorage, TempResultStorage>(); // Phase 2: Disk-based temp storage
builder.Services.AddHostedService<BatchCleanupService>();
builder.Services.Configure<AiSettings>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient<OpenAiService>();
builder.Services.AddSingleton<MockAiService>();
builder.Services.AddScoped<IAiService>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<AiSettings>>().Value;
    var hasKey = !string.IsNullOrWhiteSpace(settings.ApiKey) ||
                 (settings.FallbackProviders?.Any(p => !string.IsNullOrWhiteSpace(p.ApiKey)) ?? false);
    var provider = settings.Provider ?? "Mock";
    if ((string.Equals(provider, "OpenAI", StringComparison.OrdinalIgnoreCase) ||
         string.Equals(provider, "OpenRouter", StringComparison.OrdinalIgnoreCase)) && hasKey)
    {
        return ActivatorUtilities.CreateInstance<OpenAiService>(sp);
    }

    var logger = sp.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("AI provider using mock (Provider: {Provider}, HasKey: {HasKey})", provider, hasKey);
    return sp.GetRequiredService<MockAiService>();
});

// Add session support for admin authentication
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add runtime compilation for development (optional)
#if DEBUG
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
#endif

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

// Configure routing
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
