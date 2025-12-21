using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Services;

namespace NovaToolsHub.Controllers
{
    public class AcademicController : Controller
    {
        private readonly IAiService _aiService;
        private readonly ILogger<AcademicController> _logger;

        public AcademicController(IAiService aiService, ILogger<AcademicController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public IActionResult QuizBuilder()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Online Quiz Builder - Create Interactive Assessments | NovaTools Hub",
                MetaDescription = "Create custom quizzes and assessments with our interactive quiz builder. Perfect for teachers and students to test knowledge and track progress.",
                CanonicalUrl = Url.Action("QuizBuilder", "Academic", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        public IActionResult Flashcards()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Flashcard Generator - Study & Learn Effectively | NovaTools Hub",
                MetaDescription = "Create digital flashcards for effective studying. Save flashcard sets, review with flip animations, and master any subject.",
                CanonicalUrl = Url.Action("Flashcards", "Academic", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        public IActionResult GrammarHelper()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Grammar Helper - Check Writing & Fix Errors | NovaTools Hub",
                MetaDescription = "Improve your writing with our grammar checker. Detect spelling, punctuation, and grammar issues instantly.",
                CanonicalUrl = Url.Action("GrammarHelper", "Academic", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        public IActionResult FormulaReference()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Formula & Unit Reference - Math, Physics, Chemistry | NovaTools Hub",
                MetaDescription = "Comprehensive reference guide with formulas and units for math, physics, chemistry, and more. Essential for students and professionals.",
                CanonicalUrl = Url.Action("FormulaReference", "Academic", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        public IActionResult Whiteboard()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Interactive Whiteboard - Draw & Collaborate Online | NovaTools Hub",
                MetaDescription = "Digital whiteboard for drawing diagrams, brainstorming, and visual collaboration. Save and share your work easily.",
                CanonicalUrl = Url.Action("Whiteboard", "Academic", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        public IActionResult MindMap()
        {
            var model = new BasePageViewModel
            {
                PageTitle = "Mind Map Creator - Visualize Ideas & Concepts | NovaTools Hub",
                MetaDescription = "Create interactive mind maps to organize thoughts, plan projects, and visualize complex concepts. Save and export your mind maps.",
                CanonicalUrl = Url.Action("MindMap", "Academic", null, Request.Scheme) ?? string.Empty
            };
            return View(model);
        }

        [HttpPost]
        [Route("api/academic/grammar-check")]
        public async Task<IActionResult> GrammarCheck([FromBody] GrammarCheckRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { error = "Invalid request. Text must be between 10 and 8000 characters." });
            }

            try
            {
                _logger.LogInformation("Processing AI grammar check for text of length {Length}", request.Text.Length);
                var result = await _aiService.CheckGrammarAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Grammar check failed");
                return StatusCode(500, new { error = "Grammar check failed. Please try again later." });
            }
        }
    }
}

