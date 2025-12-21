using System.Linq;
using System.Text;
using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

/// <summary>
/// Mock AI service that generates deterministic, helpful text for demo/testing.
/// Replace with a real provider implementation when API keys are available.
/// </summary>
public class MockAiService : IAiService
{
    private static readonly string[] WritingTips =
    {
        "Keep sentences concise and focused on one idea.",
        "Use active voice where possible to improve clarity.",
        "Group related ideas into short paragraphs.",
        "Add concrete examples to make points memorable.",
        "Close with a clear next step or takeaway."
    };

    public Task<AiTextResponse> DraftAsync(AiDraftRequest request)
    {
        var paragraphs = request.Length switch
        {
            "short" => 2,
            "long" => 4,
            _ => 3
        };

        var sb = new StringBuilder();
        sb.AppendLine($"Drafting about: {request.Prompt.Trim()}");
        sb.AppendLine($"Tone: {request.Tone}. Length: {request.Length}.");
        sb.AppendLine();

        for (int i = 1; i <= paragraphs; i++)
        {
            sb.AppendLine($"{i}. {GenerateParagraph(request.Tone, request.Prompt, i)}");
            sb.AppendLine();
        }

        return Task.FromResult(new AiTextResponse
        {
            Mode = "draft",
            Tone = request.Tone,
            Length = request.Length,
            Content = sb.ToString().Trim(),
            EstimatedTokens = EstimateTokens(sb.ToString()),
            Tips = WritingTips.Take(3).ToList()
        });
    }

    public Task<AiTextResponse> SummarizeAsync(AiSummarizeRequest request)
    {
        var sentences = SplitSentences(request.Text);
        var take = request.Length switch
        {
            "brief" => 2,
            "detailed" => 5,
            _ => 3
        };

        var summary = sentences.Take(take).ToList();
        if (!summary.Any())
        {
            summary.Add("No key points detected. Provide more context for a meaningful summary.");
        }

        var result = string.Join(" ", summary);

        return Task.FromResult(new AiTextResponse
        {
            Mode = "summarize",
            Tone = "neutral",
            Length = request.Length,
            Content = $"Summary ({request.Length}): {result}",
            EstimatedTokens = EstimateTokens(result),
            Tips = new List<string>
            {
                "Shorten or simplify long sentences before summarizing.",
                "Focus on the problem, action, and outcome in each paragraph.",
                "Include the primary audience and objective when relevant."
            }
        });
    }

    public Task<AiTextResponse> ImproveAsync(AiImproveRequest request)
    {
        var improvements = new List<string>
        {
            "Simplified phrasing for clarity.",
            "Removed filler words and tightened transitions.",
            "Adjusted sentence lengths to improve rhythm.",
            "Highlighted the main takeaway earlier in the text."
        };

        var goalLine = string.IsNullOrWhiteSpace(request.Goal)
            ? string.Empty
            : $"Goal: {request.Goal.Trim()}\n\n";

        var improved = $"{goalLine}Improved version:\n{RewriteForClarity(request.Text)}";

        return Task.FromResult(new AiTextResponse
        {
            Mode = "improve",
            Tone = "neutral",
            Length = "medium",
            Content = improved,
            EstimatedTokens = EstimateTokens(improved),
            Tips = improvements
        });
    }

    public Task<GrammarCheckResponse> CheckGrammarAsync(GrammarCheckRequest request)
    {
        // Mock grammar checker that finds common issues
        var issues = new List<GrammarIssue>();
        var text = request.Text;
        var corrected = text;

        // Common spelling errors
        var spellingErrors = new Dictionary<string, string>
        {
            { "teh", "the" }, { "recieve", "receive" }, { "occured", "occurred" },
            { "seperate", "separate" }, { "definately", "definitely" }, { "acheive", "achieve" },
            { "untill", "until" }, { "occassion", "occasion" }, { "accomodate", "accommodate" }
        };

        foreach (var (wrong, right) in spellingErrors)
        {
            if (text.Contains(wrong, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new GrammarIssue
                {
                    Type = "spelling",
                    Severity = "high",
                    Original = wrong,
                    Suggestion = right,
                    Explanation = $"Common misspelling: '{wrong}' should be '{right}'"
                });
                corrected = System.Text.RegularExpressions.Regex.Replace(corrected, wrong, right, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }

        // Grammar patterns
        var grammarPatterns = new[]
        {
            ("could of", "could have", "Use 'could have' not 'could of'"),
            ("should of", "should have", "Use 'should have' not 'should of'"),
            ("would of", "would have", "Use 'would have' not 'would of'"),
            ("very unique", "unique", "'Unique' is absolute and doesn't need 'very'"),
            ("more better", "better", "Use 'better' without 'more'")
        };

        foreach (var (wrong, right, explanation) in grammarPatterns)
        {
            if (text.Contains(wrong, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new GrammarIssue
                {
                    Type = "grammar",
                    Severity = "medium",
                    Original = wrong,
                    Suggestion = right,
                    Explanation = explanation
                });
                corrected = System.Text.RegularExpressions.Regex.Replace(corrected, wrong, right, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
        }

        // Check for double spaces
        if (text.Contains("  "))
        {
            issues.Add(new GrammarIssue
            {
                Type = "style",
                Severity = "low",
                Original = "[double space]",
                Suggestion = "[single space]",
                Explanation = "Remove extra spaces between words"
            });
            corrected = System.Text.RegularExpressions.Regex.Replace(corrected, @"  +", " ");
        }

        var summary = issues.Count == 0
            ? "Great! Your text looks good. (Note: For comprehensive AI-powered checking, configure the AI API key)"
            : $"Found {issues.Count} issue(s) using basic rules. For deeper analysis, configure the AI API key.";

        return Task.FromResult(new GrammarCheckResponse
        {
            Success = true,
            CorrectedText = corrected,
            Issues = issues,
            Summary = summary
        });
    }

    private static string GenerateParagraph(string tone, string prompt, int index)
    {
        var prefix = tone switch
        {
            "professional" => "From a professional perspective,",
            "formal" => "In formal terms,",
            "friendly" => "In a friendly tone,",
            "casual" => "Casually speaking,",
            "creative" => "Imagining boldly,",
            _ => "In a clear tone,"
        };

        return $"{prefix} point {index} builds on \"{prompt}\" and gives the reader a concrete action to take.";
    }

    private static IEnumerable<string> SplitSentences(string text)
    {
        return text.Split(new[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Select(s => s + ".");
    }

    private static string RewriteForClarity(string text)
    {
        var sentences = SplitSentences(text).Take(6).ToList();
        if (!sentences.Any())
        {
            return "Provide a few sentences so the assistant can suggest improvements.";
        }

        return string.Join(" ", sentences.Select((s, i) =>
        {
            if (s.Length <= 12) return s;
            return i % 2 == 0 ? s.Replace(" very ", " ").Replace(" really ", " ") : $"* {s}";
        }));
    }

    private static int EstimateTokens(string text)
    {
        return Math.Max(50, text.Length / 4);
    }
}
