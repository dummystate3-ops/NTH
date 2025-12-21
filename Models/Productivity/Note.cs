using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models.Productivity;

/// <summary>
/// Represents a persisted note for the online notepad tool.
/// </summary>
public class Note
{
    public int Id { get; set; }

    [Required]
    public Guid WorkspaceId { get; set; }
    public ProductivityWorkspace Workspace { get; set; } = default!;

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = "Untitled Note";

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
