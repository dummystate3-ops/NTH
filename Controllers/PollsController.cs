using Microsoft.AspNetCore.Mvc;
using NovaToolsHub.Models.ViewModels;
using NovaToolsHub.Services;

namespace NovaToolsHub.Controllers;

[Route("Poll")]
public class PollsController : Controller
{
    private readonly IPollService _pollService;
    private readonly ILogger<PollsController> _logger;

    public PollsController(IPollService pollService, ILogger<PollsController> logger)
    {
        _pollService = pollService;
        _logger = logger;
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] CreatePollRequest request)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Invalid input.";
            return Json(new { success = false, error });
        }

        try
        {
            var pollId = await _pollService.CreatePollAsync(request);
            return Json(new { success = true, pollId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating poll");
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpGet("Get")]
    public async Task<IActionResult> Get(Guid id)
    {
        try
        {
            var poll = await _pollService.GetPollAsync(id);
            return Json(new { success = true, poll });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error fetching poll {PollId}", id);
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost("Vote")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Vote([FromForm] VoteRequest request)
    {
        if (!ModelState.IsValid)
        {
            var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Invalid input.";
            return Json(new { success = false, error });
        }

        var voterKey = GetVoterKey();

        try
        {
            var result = await _pollService.VoteAsync(request, voterKey);
            return Json(new { success = true, poll = result });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error voting on poll {PollId}", request.PollId);
            return Json(new { success = false, error = ex.Message });
        }
    }

    private string GetVoterKey()
    {
        const string cookieName = "PollVoterKey";
        if (Request.Cookies.TryGetValue(cookieName, out var existing) && !string.IsNullOrWhiteSpace(existing))
        {
            return existing;
        }

        var key = Guid.NewGuid().ToString("N");
        Response.Cookies.Append(cookieName, key, new CookieOptions
        {
            Expires = DateTimeOffset.UtcNow.AddYears(1),
            HttpOnly = true,
            IsEssential = true
        });
        return key;
    }
}
