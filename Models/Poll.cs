using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models;

public class Poll
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(200)]
    public string Question { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PollOption> Options { get; set; } = new List<PollOption>();
}

public class PollOption
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Text { get; set; } = string.Empty;

    public int Votes { get; set; }

    public Guid PollId { get; set; }
    public Poll? Poll { get; set; }
}

public class VoteMarker
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
