using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public interface IPollService
{
    Task<Guid> CreatePollAsync(CreatePollRequest request);
    Task<PollResultDto> GetPollAsync(Guid pollId);
    Task<PollResultDto> VoteAsync(VoteRequest request, string voterKey);
}
