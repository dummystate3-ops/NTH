using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using NovaToolsHub.Models;

namespace NovaToolsHub.Services;

public class QuizService : IQuizService
{
    private readonly ApplicationDbContext _db;
    private static readonly Random _random = new();

    public QuizService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<SharedQuizResponse> CreateQuizAsync(CreateQuizRequest request)
    {
        if (request.Questions.Count == 0)
        {
            throw new InvalidOperationException("Quiz must have at least one question.");
        }

        if (request.Questions.Count > 50)
        {
            throw new InvalidOperationException("Quiz cannot have more than 50 questions.");
        }

        // Generate unique share code
        string shareCode;
        do
        {
            shareCode = GenerateShareCode();
        } while (await _db.Set<Quiz>().AnyAsync(q => q.ShareCode == shareCode));

        var quiz = new Quiz
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ShareCode = shareCode,
            QuestionsJson = JsonSerializer.Serialize(request.Questions)
        };

        _db.Set<Quiz>().Add(quiz);
        await _db.SaveChangesAsync();

        return MapToResponse(quiz);
    }

    public async Task<SharedQuizResponse?> GetQuizByShareCodeAsync(string shareCode)
    {
        var quiz = await _db.Set<Quiz>().FirstOrDefaultAsync(q => q.ShareCode == shareCode.ToUpperInvariant());
        return quiz == null ? null : MapToResponse(quiz);
    }

    public async Task<SharedQuizResponse?> GetQuizByIdAsync(Guid id)
    {
        var quiz = await _db.Set<Quiz>().FirstOrDefaultAsync(q => q.Id == id);
        return quiz == null ? null : MapToResponse(quiz);
    }

    public async Task RecordQuizResultAsync(SubmitQuizResultRequest request)
    {
        var quiz = await _db.Set<Quiz>().FirstOrDefaultAsync(q => q.ShareCode == request.ShareCode.ToUpperInvariant());
        if (quiz == null) return;

        quiz.TimesPlayed++;
        quiz.TotalCorrectAnswers += request.CorrectAnswers;
        quiz.TotalQuestionsAnswered += request.TotalQuestions;

        await _db.SaveChangesAsync();
    }

    private static string GenerateShareCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 6).Select(_ => chars[_random.Next(chars.Length)]).ToArray());
    }

    private static SharedQuizResponse MapToResponse(Quiz quiz)
    {
        var questions = JsonSerializer.Deserialize<List<QuizQuestionDto>>(quiz.QuestionsJson) ?? new();
        
        double avgScore = 0;
        if (quiz.TotalQuestionsAnswered > 0)
        {
            avgScore = Math.Round((double)quiz.TotalCorrectAnswers / quiz.TotalQuestionsAnswered * 100, 1);
        }

        return new SharedQuizResponse
        {
            Id = quiz.Id,
            ShareCode = quiz.ShareCode,
            Title = quiz.Title,
            Description = quiz.Description,
            Questions = questions,
            CreatedAt = quiz.CreatedAt,
            TimesPlayed = quiz.TimesPlayed,
            AverageScore = avgScore
        };
    }
}
