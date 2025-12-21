using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace NovaToolsHub.Models;

/// <summary>
/// Quiz entity for shareable quizzes
/// </summary>
public class Quiz
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Short shareable code for the quiz (e.g., "ABC123")
    /// </summary>
    [Required]
    [StringLength(8)]
    public string ShareCode { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// JSON serialized questions data
    /// </summary>
    [Required]
    public string QuestionsJson { get; set; } = "[]";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int TimesPlayed { get; set; }

    public int TotalCorrectAnswers { get; set; }

    public int TotalQuestionsAnswered { get; set; }
}

/// <summary>
/// DTO for creating a quiz
/// </summary>
public class CreateQuizRequest
{
    [Required]
    [StringLength(200)]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [Required]
    [JsonPropertyName("questions")]
    public List<QuizQuestionDto> Questions { get; set; } = new();
}

/// <summary>
/// DTO for a quiz question
/// </summary>
public class QuizQuestionDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    
    [Required]
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [Required]
    [JsonPropertyName("options")]
    public List<string> Options { get; set; } = new();

    [JsonPropertyName("correctAnswer")]
    public int CorrectAnswer { get; set; }
}

/// <summary>
/// Response DTO for a shared quiz
/// </summary>
public class SharedQuizResponse
{
    public Guid Id { get; set; }
    public string ShareCode { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<QuizQuestionDto> Questions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int TimesPlayed { get; set; }
    public double AverageScore { get; set; }
}

/// <summary>
/// Request to submit quiz results
/// </summary>
public class SubmitQuizResultRequest
{
    [Required]
    [JsonPropertyName("shareCode")]
    public string ShareCode { get; set; } = string.Empty;

    [JsonPropertyName("correctAnswers")]
    public int CorrectAnswers { get; set; }

    [JsonPropertyName("totalQuestions")]
    public int TotalQuestions { get; set; }
}
