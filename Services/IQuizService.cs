using NovaToolsHub.Models;

namespace NovaToolsHub.Services;

public interface IQuizService
{
    Task<SharedQuizResponse> CreateQuizAsync(CreateQuizRequest request);
    Task<SharedQuizResponse?> GetQuizByShareCodeAsync(string shareCode);
    Task<SharedQuizResponse?> GetQuizByIdAsync(Guid id);
    Task RecordQuizResultAsync(SubmitQuizResultRequest request);
}
