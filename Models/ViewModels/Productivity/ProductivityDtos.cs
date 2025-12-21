using System.ComponentModel.DataAnnotations;

namespace NovaToolsHub.Models.ViewModels.Productivity;

public record WorkspaceResponseDto(Guid WorkspaceId, DateTime CreatedAt, DateTime LastActiveAt);

public class EnsureWorkspaceRequest
{
    public Guid? WorkspaceId { get; set; }
    [MaxLength(80)]
    public string? Name { get; set; }
}

public record TodoItemDto(
    int Id,
    string Title,
    string Priority,
    string? Category,
    bool Completed,
    DateTime CreatedAt,
    DateTime? CompletedAt);

public class CreateTodoRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "medium";

    [MaxLength(50)]
    public string? Category { get; set; }
}

public class UpdateTodoRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Priority { get; set; } = "medium";

    [MaxLength(50)]
    public string? Category { get; set; }
}

public class UpdateTodoStatusRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }
    public bool Completed { get; set; }
}

public class WorkspaceRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }
}

public record KanbanCardDto(
    int Id,
    string Title,
    string Column,
    int Position,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class CreateKanbanCardRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Column { get; set; } = "todo";
}

public class UpdateKanbanCardRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    [Required]
    [MaxLength(250)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(30)]
    public string Column { get; set; } = "todo";
}

public class MoveKanbanCardRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }
    public int Position { get; set; }

    [Required]
    [MaxLength(30)]
    public string Column { get; set; } = "todo";
}

public record NoteDto(
    int Id,
    string Title,
    string Content,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public class SaveNoteRequest
{
    [Required]
    public Guid WorkspaceId { get; set; }

    public int? NoteId { get; set; }

    [Required]
    [MaxLength(150)]
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;
}
