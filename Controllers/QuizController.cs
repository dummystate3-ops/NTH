using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models;
using NovaToolsHub.Services;

namespace NovaToolsHub.Controllers;

[Route("Quiz")]
public class QuizController : Controller
{
    private readonly IQuizService _quizService;
    private readonly ILogger<QuizController> _logger;

    public QuizController(IQuizService quizService, ILogger<QuizController> logger)
    {
        _quizService = quizService;
        _logger = logger;
    }

    /// <summary>
    /// Create and share a new quiz
    /// </summary>
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] JsonElement payload)
    {
        CreateQuizRequest? request;
        try
        {
            request = JsonSerializer.Deserialize<CreateQuizRequest>(payload.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Invalid quiz payload");
            return Json(new { success = false, error = "Invalid quiz format." });
        }

        if (request == null)
        {
            return Json(new { success = false, error = "Quiz data is missing." });
        }

        ModelState.Clear();
        if (!TryValidateModel(request))
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid input." : e.ErrorMessage)
                .ToList();
            var errorMessage = errors.Count == 0 ? "Invalid input." : string.Join(" ", errors);
            return Json(new { success = false, error = errorMessage });
        }

        try
        {
            var quiz = await _quizService.CreateQuizAsync(request);
            var shareUrl = Url.Action("Take", "Quiz", new { code = quiz.ShareCode }, Request.Scheme);
            return Json(new { success = true, quiz, shareUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating quiz");
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get quiz by share code (API)
    /// </summary>
    [HttpGet("Get/{code}")]
    public async Task<IActionResult> Get(string code)
    {
        try
        {
            var quiz = await _quizService.GetQuizByShareCodeAsync(code);
            if (quiz == null)
            {
                return Json(new { success = false, error = "Quiz not found." });
            }
            return Json(new { success = true, quiz });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching quiz {Code}", code);
            return Json(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Take a shared quiz (view)
    /// </summary>
    [HttpGet("Take/{code}")]
    public async Task<IActionResult> Take(string code)
    {
        var quiz = await _quizService.GetQuizByShareCodeAsync(code);
        if (quiz == null)
        {
            return RedirectToAction("QuizBuilder", "Academic");
        }
        return View(quiz);
    }

    /// <summary>
    /// Submit quiz results to update statistics
    /// </summary>
    [HttpPost("SubmitResult")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitResult([FromBody] SubmitQuizResultRequest request)
    {
        try
        {
            await _quizService.RecordQuizResultAsync(request);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error recording quiz result");
            return Json(new { success = false, error = ex.Message });
        }
    }
}
