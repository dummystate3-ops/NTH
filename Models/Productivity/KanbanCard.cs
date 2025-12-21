using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models.Productivity;

/// <summary>
/// Represents a Kanban board card belonging to a workspace.
/// </summary>
public class KanbanCard
{
    public int Id { get; set; }

    [Required]
    public Guid WorkspaceId { get; set; }
    public ProductivityWorkspace Workspace { get; set; } = default!;

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Column { get; set; } = "todo";

    public int Position { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
