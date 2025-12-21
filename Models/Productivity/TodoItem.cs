using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models.Productivity;

/// <summary>
/// Server-side representation of a to-do list item.
/// </summary>
public class TodoItem
{
    public int Id { get; set; }

    [Required]
    public Guid WorkspaceId { get; set; }
    public ProductivityWorkspace Workspace { get; set; } = default!;

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "medium";

    [MaxLength(50)]
    public string? Category { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
