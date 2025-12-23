using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Services;
using NovaToolsHub.Helpers;
using System.Collections.Generic;

namespace NovaToolsHub.Controllers
{
    public class AcademicController : Controller
    {
        private readonly IAiService _aiService;
        private readonly ILogger<AcademicController> _logger;iService _aiService;
        private readonly ILogger<AcademicController> _logger;

        public AcademicController(IAiService aiService, ILogger<AcademicController> logger)
        {
            _aiService = aiService;
            _logger = logger;
        }

        public IActionResult QuizBuilder()
        {
            var url = Url.Action("QuizBuilder", "Academic", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Online Quiz Builder - Create Interactive Assessments | NovaTools Hub",
                MetaDescription = "Create custom quizzes and assessments with our interactive quiz builder. Perfect for teachers and students to test knowledge and track progress.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Online Quiz Builder",
                "Create custom quizzes and assessments with multiple choice questions, saved locally, and shareable quiz links.",
                url,
                "EducationalApplication"
            );

            var faqs = new List&lt;(string Question, string Answer)&gt;
            {
                (
                    "Where are my quizzes saved?",
                    "Quizzes you build are stored in your browser using local storage on this device. They are not saved to a central account or automatically synced across different devices or browsers."
                ),
                (
                    "Can I share a quiz with students or friends?",
                    "Yes. Use the share option to generate a share link and code. Anyone with the link can open the quiz in their browser and take it without needing an account."
                ),
                (
                    "Is there a limit on the number of questions per quiz?",
                    "There is no strict built-in limit, but very large quizzes may become harder to manage or slightly slower in the browser. For the best experience, keep quizzes to a reasonable size."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        public IActionResult Flashcards()
        {
            var url = Url.Action("Flashcards", "Academic", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Flashcard Generator - Study & Learn Effectively | NovaTools Hub",
                MetaDescription = "Create digital flashcards for effective studying. Save flashcard sets, review with flip animations, and master any subject.",
                CanonicalUrl = url
            };

            var learningSchema = SeoHelper.GenerateLearningResourceSchema(
                "Digital Flashcard Generator",
                "Create, save, and review digital flashcard sets for effective spaced repetition and practice.",
                url,
                "Student"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Where are my flashcard sets stored?",
                    "Flashcard sets are saved in your browser’s local storage on this device. They stay available in this browser even if you close the tab, but they are not synced across devices."
                ),
                (
                    "Can I export or import flashcard sets?",
                    "Yes. You can export a set as a JSON file and later import it back into the tool, or share it with someone else who can import it into their own browser."
                ),
                (
                    "Is there a limit to how many cards a set can contain?",
                    "There is no hard coded limit, but very large sets may feel slower to load or navigate. In practice, using focused sets of a few dozen cards tends to work best for studying."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{learningSchema},{faqSchema}]";

            return View(model);
        }

        public IActionResult GrammarHelper()
        {
            var url = Url.Action("GrammarHelper", "Academic", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Grammar Helper - Check Writing & Fix Errors | NovaTools Hub",
                MetaDescription = "Improve your writing with our grammar checker. Detect spelling, punctuation, and grammar issues instantly.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Grammar Helper",
                "AI-powered grammar, spelling, and style checker for improving the clarity and correctness of your writing.",
                url,
                "EducationalApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "What does the AI grammar helper check?",
                    "The tool analyzes your text for grammar, spelling, punctuation, and basic style issues. It returns a summary, corrected text where possible, and a list of issues with explanations."
                ),
                (
                    "Is my text stored or used for training models?",
                    "Your text is sent securely to the AI service in order to generate suggestions. NovaTools Hub does not intentionally store your text long-term, but you should still avoid pasting highly sensitive or secret information."
                ),
                (
                    "Can this replace a human editor?",
                    "No. It is a helpful assistant for catching many common issues, but it does not fully understand context, nuance, or domain expertise. For important documents, you should still review suggestions critically or consult a human editor."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        public IActionResult FormulaReference()
        {
            var url = Url.Action("FormulaReference", "Academic", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Formula & Unit Reference - Math, Physics, Chemistry | NovaTools Hub",
                MetaDescription = "Comprehensive reference guide with formulas and units for math, physics, chemistry, and more. Essential for students and professionals.",
                CanonicalUrl = url
            };

            var learningSchema = SeoHelper.GenerateLearningResourceSchema(
                "Formula & Unit Reference",
                "Quick reference for common formulas and units in mathematics, physics, chemistry, and geometry.",
                url,
                "Student"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Can I rely on this page as my only formula reference?",
                    "This page is designed as a convenient quick reference for many commonly used formulas. For exams, assignments, or research, you should always cross-check with your textbook, course notes, or official reference materials."
                ),
                (
                    "Do you cover every possible formula?",
                    "No. The reference focuses on frequently used formulas across math, physics, chemistry, and geometry. It is not an exhaustive list of all formulas in those subjects."
                ),
                (
                    "How should I cite formulas from this tool?",
                    "Formulas themselves are standard and usually do not require citation, but any explanations or wording should be cited from your primary learning resources (such as textbooks or lecture notes) rather than this summary page."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{learningSchema},{faqSchema}]";

            return View(model);
        }

        public IActionResult Whiteboard()
        {
            var url = Url.Action("Whiteboard", "Academic", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Interactive Whiteboard - Draw & Collaborate Online | NovaTools Hub",
                MetaDescription = "Digital whiteboard for drawing diagrams, brainstorming, and visual collaboration. Save and share your work easily.",
                CanonicalUrl = url
            };

            var appSchema = SeoHelper.GenerateSoftwareApplicationSchema(
                "Interactive Whiteboard",
                "Browser-based interactive whiteboard for drawing diagrams, sketching ideas, and saving snapshots locally.",
                url,
                "EducationalApplication"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Where are my whiteboard drawings stored?",
                    "Saved drawings are stored in your browser’s local storage as image data. They remain available in this browser but are not synced to other devices or accounts."
                ),
                (
                    "Can I export my whiteboard as an image?",
                    "Yes. You can export the current whiteboard as a PNG file and download it, which is useful for including diagrams in notes, slides, or documents."
                ),
                (
                    "Does this whiteboard support real-time collaboration?",
                    "No. The current version is designed for individual use in a single browser. It does not provide multi-user, real-time collaboration features."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{appSchema},{faqSchema}]";

            return View(model);
        }

        public IActionResult MindMap()
        {
            var url = Url.Action("MindMap", "Academic", null, Request.Scheme) ?? string.Empty;

            var model = new BasePageViewModel
            {
                PageTitle = "Mind Map Creator - Visualize Ideas & Concepts | NovaTools Hub",
                MetaDescription = "Create interactive mind maps to organize thoughts, plan projects, and visualize complex concepts. Save and export your mind maps.",
                CanonicalUrl = url
            };

            var learningSchema = SeoHelper.GenerateLearningResourceSchema(
                "Mind Map Creator",
                "Create, edit, and save interactive mind maps in your browser to organize ideas, projects, and study topics.",
                url,
                "Student"
            );

            var faqs = new List<(string Question, string Answer)>
            {
                (
                    "Where are my mind maps saved?",
                    "Mind maps are stored in your browser’s local storage on this device. They are available when you return to the same browser, but they are not automatically synced elsewhere."
                ),
                (
                    "Can I export or import mind maps?",
                    "Yes. You can export a mind map as a JSON file and later import it back into the tool, or share it with someone who can load it into their own browser."
                ),
                (
                    "Is the tool suitable for very large mind maps?",
                    "You can create fairly large maps, but extremely complex diagrams may become harder to navigate or slower to render in the browser. For best usability, keep each map focused on a specific topic or project."
                )
            };

            var faqSchema = SeoHelper.GenerateFaqPageSchema(faqs);
            ViewBag.JsonLdSchema = $"[{learningSchema},{faqSchema}]";

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

