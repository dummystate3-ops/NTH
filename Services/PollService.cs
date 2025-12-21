using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using NovaToolsHub.Models;
using NovaToolsHub.Models.ViewModels;

namespace NovaToolsHub.Services;

public class PollService : IPollService
{
    private readonly ApplicationDbContext _db;

    public PollService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Guid> CreatePollAsync(CreatePollRequest request)
    {
        var poll = new Poll
        {
            Question = request.Question.Trim()
        };

        foreach (var opt in request.Options.Where(o => !string.IsNullOrWhiteSpace(o)))
        {
            poll.Options.Add(new PollOption { Text = opt.Trim() });
        }

        if (poll.Options.Count < 2 || poll.Options.Count > 10)
        {
            throw new InvalidOperationException("Poll must have between 2 and 10 options.");
        }

        _db.Polls.Add(poll);
        await _db.SaveChangesAsync();
        return poll.Id;
    }

    public async Task<PollResultDto> GetPollAsync(Guid pollId)
    {
        var poll = await _db.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == pollId);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");
        return MapToResult(poll);
    }

    public async Task<PollResultDto> VoteAsync(VoteRequest request, string voterKey)
    {
        var poll = await _db.Polls.Include(p => p.Options).FirstOrDefaultAsync(p => p.Id == request.PollId);
        if (poll == null) throw new KeyNotFoundException("Poll not found.");

        var option = poll.Options.FirstOrDefault(o => o.Id == request.OptionId);
        if (option == null) throw new KeyNotFoundException("Option not found.");

        // Basic double-vote guard per voterKey + poll
        var voteKey = $"PollVote_{request.PollId}_{voterKey}";
        if (await _db.Set<VoteMarker>().AnyAsync(v => v.Key == voteKey))
        {
            throw new InvalidOperationException("You have already voted on this poll.");
        }

        option.Votes++;
        _db.Set<VoteMarker>().Add(new VoteMarker { Key = voteKey, CreatedAt = DateTime.UtcNow });
        await _db.SaveChangesAsync();

        return MapToResult(poll);
    }

    private static PollResultDto MapToResult(Poll poll)
    {
        return new PollResultDto
        {
            PollId = poll.Id,
            Question = poll.Question,
            Options = poll.Options
                .OrderByDescending(o => o.Votes)
                .Select(o => new PollOptionResultDto
                {
                    OptionId = o.Id,
                    Text = o.Text,
                    Votes = o.Votes
                })
                .ToList()
        };
    }
}
