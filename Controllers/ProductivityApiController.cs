using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Data;
using NovaToolsHub.Models.Productivity;
using NovaToolsHub.Models.ViewModels.Productivity;

namespace NovaToolsHub.Controllers;

[ApiController]
[Route("api/productivity")]
public class ProductivityApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ProductivityApiController> _logger;

    public ProductivityApiController(ApplicationDbContext db, ILogger<ProductivityApiController> logger)
    {
        _db = db;
        _logger = logger;
    }

    #region Workspace
    [HttpPost("workspaces/ensure")]
    public async Task<ActionResult<WorkspaceResponseDto>> EnsureWorkspace([FromBody] EnsureWorkspaceRequest request)
    {
        if (request == null)
        {
            return BadRequest(new { error = "Request body is required." });
        }

        var now = DateTime.UtcNow;
        ProductivityWorkspace? workspace = null;

        if (request.WorkspaceId.HasValue)
        {
            workspace = await _db.ProductivityWorkspaces.FirstOrDefaultAsync(w => w.Id == request.WorkspaceId.Value);
            if (workspace != null)
            {
                workspace.LastActiveAt = now;
                if (!string.IsNullOrWhiteSpace(request.Name))
                {
                    workspace.Name = request.Name!.Trim();
                }
                await _db.SaveChangesAsync();
                return Ok(MapWorkspace(workspace));
            }
        }

        workspace = new ProductivityWorkspace
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(request.Name) ? null : request.Name!.Trim(),
            CreatedAt = now,
            LastActiveAt = now
        };

        _db.ProductivityWorkspaces.Add(workspace);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Created new productivity workspace {WorkspaceId}", workspace.Id);
        return Ok(MapWorkspace(workspace));
    }
    #endregion

    #region Todo Items
    [HttpGet("workspaces/{workspaceId:guid}/todos")]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetTodos(Guid workspaceId)
    {
        var workspace = await GetWorkspaceAsync(workspaceId);
        if (workspace == null) return NotFound(new { error = "Workspace not found." });

        workspace.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var items = await _db.TodoItems
            .Where(t => t.WorkspaceId == workspaceId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => new TodoItemDto(t.Id, t.Title, t.Priority, t.Category, t.IsCompleted, t.CreatedAt, t.CompletedAt))
            .ToListAsync();

        return Ok(items);
    }

    [HttpPost("workspaces/{workspaceId:guid}/todos")]
    public async Task<ActionResult<TodoItemDto>> CreateTodo(Guid workspaceId, [FromBody] CreateTodoRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });
        if (request.WorkspaceId != workspaceId) return BadRequest(new { error = "Workspace mismatch." });

        var workspace = await GetWorkspaceAsync(workspaceId);
        if (workspace == null) return NotFound(new { error = "Workspace not found." });

        var item = new TodoItem
        {
            WorkspaceId = workspaceId,
            Title = request.Title.Trim(),
            Priority = request.Priority.Trim().ToLowerInvariant(),
            Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.TodoItems.Add(item);
        workspace.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodos), new { workspaceId }, MapTodo(item));
    }

    [HttpPut("todos/{id:int}")]
    public async Task<ActionResult<TodoItemDto>> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });

        var item = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == request.WorkspaceId);
        if (item == null) return NotFound(new { error = "Todo item not found." });

        item.Title = request.Title.Trim();
        item.Priority = request.Priority.Trim().ToLowerInvariant();
        item.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
        item.UpdatedAt = DateTime.UtcNow;

        await TouchWorkspace(request.WorkspaceId);
        await _db.SaveChangesAsync();

        return Ok(MapTodo(item));
    }

    [HttpPatch("todos/{id:int}/status")]
    public async Task<ActionResult<TodoItemDto>> UpdateTodoStatus(int id, [FromBody] UpdateTodoStatusRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });

        var item = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == request.WorkspaceId);
        if (item == null) return NotFound(new { error = "Todo item not found." });

        item.IsCompleted = request.Completed;
        item.CompletedAt = request.Completed ? DateTime.UtcNow : null;
        item.UpdatedAt = DateTime.UtcNow;

        await TouchWorkspace(request.WorkspaceId);
        await _db.SaveChangesAsync();

        return Ok(MapTodo(item));
    }

    [HttpDelete("todos/{id:int}")]
    public async Task<IActionResult> DeleteTodo(int id, [FromQuery] Guid workspaceId)
    {
        var item = await _db.TodoItems.FirstOrDefaultAsync(t => t.Id == id && t.WorkspaceId == workspaceId);
        if (item == null) return NotFound(new { error = "Todo item not found." });

        _db.TodoItems.Remove(item);
        await TouchWorkspace(workspaceId);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("workspaces/{workspaceId:guid}/todos/clear-completed")]
    public async Task<ActionResult<object>> ClearCompletedTodos(Guid workspaceId)
    {
        var completed = await _db.TodoItems.Where(t => t.WorkspaceId == workspaceId && t.IsCompleted).ToListAsync();
        if (completed.Count == 0)
        {
            return Ok(new { deleted = 0 });
        }

        _db.TodoItems.RemoveRange(completed);
        await TouchWorkspace(workspaceId);
        await _db.SaveChangesAsync();

        return Ok(new { deleted = completed.Count });
    }
    #endregion

    #region Kanban
    [HttpGet("workspaces/{workspaceId:guid}/kanban")]
    public async Task<ActionResult<IEnumerable<KanbanCardDto>>> GetKanban(Guid workspaceId)
    {
        var workspace = await GetWorkspaceAsync(workspaceId);
        if (workspace == null) return NotFound(new { error = "Workspace not found." });

        workspace.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var cards = await _db.KanbanCards
            .Where(c => c.WorkspaceId == workspaceId)
            .OrderBy(c => c.Column)
            .ThenBy(c => c.Position)
            .Select(c => new KanbanCardDto(c.Id, c.Title, c.Column, c.Position, c.CreatedAt, c.UpdatedAt))
            .ToListAsync();

        return Ok(cards);
    }

    [HttpPost("workspaces/{workspaceId:guid}/kanban")]
    public async Task<ActionResult<KanbanCardDto>> CreateCard(Guid workspaceId, [FromBody] CreateKanbanCardRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });
        if (request.WorkspaceId != workspaceId) return BadRequest(new { error = "Workspace mismatch." });

        var workspace = await GetWorkspaceAsync(workspaceId);
        if (workspace == null) return NotFound(new { error = "Workspace not found." });

        var column = request.Column.Trim().ToLowerInvariant();
        var nextPosition = await _db.KanbanCards
            .Where(c => c.WorkspaceId == workspaceId && c.Column == column)
            .CountAsync();

        var card = new KanbanCard
        {
            WorkspaceId = workspaceId,
            Title = request.Title.Trim(),
            Column = column,
            Position = nextPosition,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.KanbanCards.Add(card);
        workspace.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetKanban), new { workspaceId }, MapCard(card));
    }

    [HttpPut("kanban/{id:int}")]
    public async Task<ActionResult<KanbanCardDto>> UpdateCard(int id, [FromBody] UpdateKanbanCardRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });

        var card = await _db.KanbanCards.FirstOrDefaultAsync(c => c.Id == id && c.WorkspaceId == request.WorkspaceId);
        if (card == null) return NotFound(new { error = "Kanban card not found." });

        var originalColumn = card.Column;
        card.Title = request.Title.Trim();
        card.Column = request.Column.Trim().ToLowerInvariant();
        card.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        if (!string.Equals(originalColumn, card.Column, StringComparison.Ordinal))
        {
            await NormalizeColumnOrdering(request.WorkspaceId, originalColumn);
        }

        await NormalizeColumnOrdering(request.WorkspaceId, card.Column);
        await TouchWorkspace(request.WorkspaceId);
        await _db.SaveChangesAsync();

        return Ok(MapCard(card));
    }

    [HttpPatch("kanban/{id:int}/move")]
    public async Task<ActionResult<KanbanCardDto>> MoveCard(int id, [FromBody] MoveKanbanCardRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });

        var card = await _db.KanbanCards.FirstOrDefaultAsync(c => c.Id == id && c.WorkspaceId == request.WorkspaceId);
        if (card == null) return NotFound(new { error = "Kanban card not found." });

        card.Column = request.Column.Trim().ToLowerInvariant();
        card.UpdatedAt = DateTime.UtcNow;

        await ReorderCard(card, request.Position);
        await TouchWorkspace(request.WorkspaceId);
        await _db.SaveChangesAsync();

        return Ok(MapCard(card));
    }

    [HttpDelete("kanban/{id:int}")]
    public async Task<IActionResult> DeleteCard(int id, [FromQuery] Guid workspaceId)
    {
        var card = await _db.KanbanCards.FirstOrDefaultAsync(c => c.Id == id && c.WorkspaceId == workspaceId);
        if (card == null) return NotFound(new { error = "Kanban card not found." });

        var column = card.Column;
        _db.KanbanCards.Remove(card);
        await _db.SaveChangesAsync();

        await NormalizeColumnOrdering(workspaceId, column);
        await TouchWorkspace(workspaceId);
        await _db.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region Notes
    [HttpGet("workspaces/{workspaceId:guid}/notes")]
    public async Task<ActionResult<IEnumerable<NoteDto>>> GetNotes(Guid workspaceId)
    {
        var workspace = await GetWorkspaceAsync(workspaceId);
        if (workspace == null) return NotFound(new { error = "Workspace not found." });

        workspace.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        var notes = await _db.Notes
            .Where(n => n.WorkspaceId == workspaceId)
            .OrderByDescending(n => n.UpdatedAt)
            .Select(n => new NoteDto(n.Id, n.Title, n.Content, n.CreatedAt, n.UpdatedAt))
            .ToListAsync();

        return Ok(notes);
    }

    [HttpGet("notes/{id:int}")]
    public async Task<ActionResult<NoteDto>> GetNote(int id, [FromQuery] Guid workspaceId)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.WorkspaceId == workspaceId);
        if (note == null) return NotFound(new { error = "Note not found." });
        await TouchWorkspace(workspaceId);
        await _db.SaveChangesAsync();
        return Ok(MapNote(note));
    }

    [HttpPost("workspaces/{workspaceId:guid}/notes")]
    public async Task<ActionResult<NoteDto>> CreateNote(Guid workspaceId, [FromBody] SaveNoteRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });
        if (request.WorkspaceId != workspaceId) return BadRequest(new { error = "Workspace mismatch." });

        var workspace = await GetWorkspaceAsync(workspaceId);
        if (workspace == null) return NotFound(new { error = "Workspace not found." });

        var note = new Note
        {
            WorkspaceId = workspaceId,
            Title = request.Title.Trim(),
            Content = request.Content ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _db.Notes.Add(note);
        workspace.LastActiveAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotes), new { workspaceId }, MapNote(note));
    }

    [HttpPut("notes/{id:int}")]
    public async Task<ActionResult<NoteDto>> UpdateNote(int id, [FromBody] SaveNoteRequest request)
    {
        if (request == null) return BadRequest(new { error = "Request body is required." });

        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.WorkspaceId == request.WorkspaceId);
        if (note == null) return NotFound(new { error = "Note not found." });

        note.Title = request.Title.Trim();
        note.Content = request.Content ?? string.Empty;
        note.UpdatedAt = DateTime.UtcNow;

        await TouchWorkspace(request.WorkspaceId);
        await _db.SaveChangesAsync();

        return Ok(MapNote(note));
    }

    [HttpDelete("notes/{id:int}")]
    public async Task<IActionResult> DeleteNote(int id, [FromQuery] Guid workspaceId)
    {
        var note = await _db.Notes.FirstOrDefaultAsync(n => n.Id == id && n.WorkspaceId == workspaceId);
        if (note == null) return NotFound(new { error = "Note not found." });

        _db.Notes.Remove(note);
        await TouchWorkspace(workspaceId);
        await _db.SaveChangesAsync();

        return NoContent();
    }
    #endregion

    #region Helpers
    private static WorkspaceResponseDto MapWorkspace(ProductivityWorkspace workspace) =>
        new(workspace.Id, workspace.CreatedAt, workspace.LastActiveAt);

    private static TodoItemDto MapTodo(TodoItem item) =>
        new(item.Id, item.Title, item.Priority, item.Category, item.IsCompleted, item.CreatedAt, item.CompletedAt);

    private static KanbanCardDto MapCard(KanbanCard card) =>
        new(card.Id, card.Title, card.Column, card.Position, card.CreatedAt, card.UpdatedAt);

    private static NoteDto MapNote(Note note) =>
        new(note.Id, note.Title, note.Content, note.CreatedAt, note.UpdatedAt);

    private async Task<ProductivityWorkspace?> GetWorkspaceAsync(Guid workspaceId) =>
        await _db.ProductivityWorkspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);

    private async Task TouchWorkspace(Guid workspaceId)
    {
        var workspace = await _db.ProductivityWorkspaces.FirstOrDefaultAsync(w => w.Id == workspaceId);
        if (workspace != null)
        {
            workspace.LastActiveAt = DateTime.UtcNow;
        }
    }

    private async Task NormalizeColumnOrdering(Guid workspaceId, string column)
    {
        var cards = await _db.KanbanCards
            .Where(c => c.WorkspaceId == workspaceId && c.Column == column)
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Id)
            .ToListAsync();

        for (var i = 0; i < cards.Count; i++)
        {
            if (cards[i].Position != i)
            {
                cards[i].Position = i;
                cards[i].UpdatedAt = DateTime.UtcNow;
            }
        }
    }

    private async Task ReorderCard(KanbanCard card, int targetPosition)
    {
        var cards = await _db.KanbanCards
            .Where(c => c.WorkspaceId == card.WorkspaceId && c.Column == card.Column)
            .OrderBy(c => c.Position)
            .ThenBy(c => c.Id)
            .ToListAsync();

        cards.RemoveAll(c => c.Id == card.Id);
        var insertIndex = Math.Clamp(targetPosition, 0, cards.Count);
        cards.Insert(insertIndex, card);

        for (var i = 0; i < cards.Count; i++)
        {
            if (cards[i].Position != i)
            {
                cards[i].Position = i;
                cards[i].UpdatedAt = DateTime.UtcNow;
            }
        }
    }
    #endregion
}
