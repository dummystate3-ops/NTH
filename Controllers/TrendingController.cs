using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Services;
using NovaToolsHub.Helpers;
using System.IO;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace NovaToolsHub.Controllers
{
    public class TrendingController : Controller
    {
        private readonly IAiService _aiService;
        private readonly IEncryptionService _encryptionService;
        private readonly IMemeService _memeService;
        private readonly IRecipeService _recipeService;
        private readonly IRateComparisonService _rateComparisonService;
        private readonly ILogger<TrendingController> _logger;

        public TrendingController(IAiService aiService, IEncryptionService encryptionService, IMemeService memeService, IRecipeService recipeService, IRateComparisonService rateComparisonService, ILogger<TrendingController> logger)
        {
            _aiService = aiService ?? throw new ArgumentNullException(nameof(aiService));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _memeService = memeService ?? throw new ArgumentNullException(nameof(memeService));
            _recipeService = recipeService ?? throw new ArgumentNullException(nameof(recipeService));
            _rateComparisonService = rateComparisonService ?? throw new ArgumentNullException(nameof(rateComparisonService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // AI Writing Assistant/Summarizer
        public IActionResult AIWritingAssistant()
        {
            var url = Url.Action("AIWritingAssistant", "Trending", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "AI Writing Assistant & Summarizer - NovaTools Hub",
                MetaDescription = "AI-powered writing assistant to draft content, summarize text, and improve your writing. Get instant AI suggestions and summaries.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "AI Writing Assistant & Summarizer",
                "AI-powered writing assistant to draft content, summarize text, and improve your writing with instant suggestions.",
                url,
                "ProductivityApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What can I use the AI writing assistant for?",
                    "You can use the assistant to draft new content from prompts, summarize existing text to different lengths, and improve clarity, tone, or structure of your writing. It is ideal for emails, blog posts, landing page copy, and quick summaries."
                ),
                (
                    "Is my text stored or reused when I use this tool?",
                    "Prompts and responses are processed on the server to generate AI output, but the tool is not designed to store your content long term. As a best practice, avoid including highly sensitive personal data, passwords, or confidential production information in prompts."
                ),
                (
                    "Is AI-generated content ready to publish without edits?",
                    "AI output can be a strong starting point but may contain inaccuracies or phrasing that does not match your brand voice. Always review, fact-check, and edit AI-generated content before publishing or sending it to others."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Draft([FromForm] AiDraftRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(Draft));
            }

            try
            {
                var result = await _aiService.DraftAsync(request);
                return Json(new { success = true, text = result.Content, meta = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating AI draft");
                return Json(new { success = false, error = "Unable to generate a draft right now. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Summarize([FromForm] AiSummarizeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(Summarize));
            }

            try
            {
                var result = await _aiService.SummarizeAsync(request);
                return Json(new { success = true, text = result.Content, meta = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error summarizing text");
                return Json(new { success = false, error = "Unable to summarize right now. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Improve([FromForm] AiImproveRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(Improve));
            }

            try
            {
                var result = await _aiService.ImproveAsync(request);
                return Json(new { success = true, text = result.Content, meta = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error improving text");
                return Json(new { success = false, error = "Unable to improve the text right now. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Encrypt([FromForm] EncryptionRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(Encrypt));
            }

            try
            {
                var cipher = await _encryptionService.EncryptAsync(model.Text, model.Password);
                return Json(new { success = true, cipher });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Encryption failed");
                return Json(new { success = false, error = "Encryption failed. Please try again." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrypt([FromForm] EncryptionRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(Decrypt));
            }

            try
            {
                var plain = await _encryptionService.DecryptAsync(model.Text, model.Password);
                return Json(new { success = true, plain });
            }
            catch (CryptographicException)
            {
                _logger.LogWarning("Decryption failed due to invalid cipher or password.");
                return Json(new { success = false, error = "Decryption failed. Password or data is invalid." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Decryption failed unexpectedly");
                return Json(new { success = false, error = "Decryption failed. Please try again." });
            }
        }

        // Currency/Unit Rate Comparison
        public IActionResult RateComparison()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Currency & Rate Comparison Tool - NovaTools Hub",
                MetaDescription = "Compare multiple currencies and units side-by-side. Real-time exchange rates and unit conversions for easy comparison.",
                CanonicalUrl = Url.Action("RateComparison", "Trending", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompareRates([FromForm] RateComparisonRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(CompareRates));
            }

            try
            {
                var result = await _rateComparisonService.CompareAsync(request);
                return Json(new { success = true, data = result });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid rate comparison request");
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error comparing rates");
                return Json(new { success = false, error = "Unable to compare rates right now. Please try again." });
            }
        }

        // Quick Poll/Survey Builder
        public IActionResult PollBuilder()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Quick Poll & Survey Builder - NovaTools Hub",
                MetaDescription = "Create instant polls and surveys with visual results. Build engaging polls with charts and share results easily.",
                CanonicalUrl = Url.Action("PollBuilder", "Trending", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        // Data Privacy Tool (Encrypt/Decrypt)
        public IActionResult EncryptionTool()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Data Encryption & Privacy Tool - NovaTools Hub",
                MetaDescription = "Encrypt and decrypt text securely in your browser. AES encryption for protecting sensitive data. All processing happens locally.",
                CanonicalUrl = Url.Action("EncryptionTool", "Trending", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        // Meme Generator
        public IActionResult MemeGenerator()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Meme Generator - Create & Share Memes - NovaTools Hub",
                MetaDescription = "Create custom memes with our easy meme generator. Add text to images, download, and share your memes instantly.",
                CanonicalUrl = Url.Action("MemeGenerator", "Trending", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateMeme([FromForm] MemeRequest request)
        {
            if (request.BaseImage == null || request.BaseImage.Length == 0)
            {
                ModelState.AddModelError(nameof(request.BaseImage), "Upload an image or choose a template.");
            }

            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(GenerateMeme));
            }

            try
            {
                var response = await _memeService.GenerateAsync(request);
                var downloadKey = Guid.NewGuid().ToString("N");
                HttpContext.Session.SetString($"Meme_{downloadKey}_Data", response.Base64Image);
                HttpContext.Session.SetString($"Meme_{downloadKey}_ContentType", response.ContentType);
                HttpContext.Session.SetString($"Meme_{downloadKey}_FileName", response.FileName);

                return Json(new
                {
                    success = true,
                    imageData = response.Base64Image,
                    contentType = response.ContentType,
                    fileName = response.FileName,
                    downloadKey
                });
            }
            catch (InvalidDataException ex)
            {
                _logger.LogWarning(ex, "Invalid meme request");
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating meme");
                return Json(new { success = false, error = "Unable to create your meme right now. Please try again in a moment." });
            }
        }

        [HttpGet]
        [Route("Trending/Meme/Download")]
        public IActionResult DownloadMeme(string? key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return NotFound("Invalid download key.");
            }

            var base64 = HttpContext.Session.GetString($"Meme_{key}_Data");
            if (string.IsNullOrEmpty(base64))
            {
                return NotFound("Meme not found or expired. Please generate a new meme.");
            }

            var contentType = HttpContext.Session.GetString($"Meme_{key}_ContentType") ?? "image/png";
            var fileName = HttpContext.Session.GetString($"Meme_{key}_FileName") ?? $"nova-meme-{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            var bytes = Convert.FromBase64String(base64);
            return File(bytes, contentType, fileName);
        }

        // Recipe Generator
        public IActionResult RecipeGenerator()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "AI Recipe Generator - NovaTools Hub",
                MetaDescription = "Generate delicious recipes from your ingredients. AI-powered recipe suggestions with complete instructions and ingredient lists.",
                CanonicalUrl = Url.Action("RecipeGenerator", "Trending", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecipe([FromForm] RecipeRequest request)
        {
            if (!ModelState.IsValid)
            {
                return ValidationProblem(nameof(GenerateRecipe));
            }

            try
            {
                var recipe = await _recipeService.GenerateAsync(request);
                return Json(new { success = true, recipe });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid recipe request");
                return Json(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recipe");
                return Json(new { success = false, error = "We couldn't generate a recipe right now. Please try again." });
            }
        }

        private string GetFirstError()
        {
            return ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Invalid input.";
        }

        private JsonResult ValidationProblem(string actionName)
        {
            var error = GetFirstError();
            _logger.LogWarning("Validation failed for {Action}: {Error}", actionName, error);
            return Json(new { success = false, error });
        }
    }
}
