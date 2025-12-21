using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models.Productivity;

/// <summary>
/// Represents a logical bucket that groups all productivity data for a single browser/device.
/// </summary>
public class ProductivityWorkspace
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(80)]
    public string? Name { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    public ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
    public ICollection<KanbanCard> KanbanCards { get; set; } = new List<KanbanCard>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();
}
