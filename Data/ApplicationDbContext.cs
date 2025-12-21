using Microsoft.EntityFrameworkCore;
using NovaToolsHub.Models;
using NovaToolsHub.Models.Productivity;

namespace NovaToolsHub.Data;

/// <summary>
/// Application database context for NovaTools Hub
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<BlogPost> BlogPosts { get; set; } = default!;
    public DbSet<BlogCategory> BlogCategories { get; set; } = default!;
    public DbSet<Poll> Polls { get; set; } = default!;
    public DbSet<PollOption> PollOptions { get; set; } = default!;
    public DbSet<VoteMarker> VoteMarkers { get; set; } = default!;
    public DbSet<Quiz> Quizzes { get; set; } = default!;

    public DbSet<ProductivityWorkspace> ProductivityWorkspaces { get; set; } = default!;
    public DbSet<TodoItem> TodoItems { get; set; } = default!;
    public DbSet<KanbanCard> KanbanCards { get; set; } = default!;
    public DbSet<Note> Notes { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Blog Post
        modelBuilder.Entity<BlogPost>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
            entity.Property(e => e.MetaDescription).HasMaxLength(500);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.PublishedDate);
        });

        // Configure Blog Category
        modelBuilder.Entity<BlogCategory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        modelBuilder.Entity<ProductivityWorkspace>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.LastActiveAt).HasPrecision(0);
            entity.Property(e => e.Name).HasMaxLength(80);
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Priority).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.CompletedAt).HasPrecision(0);
            entity.HasIndex(e => e.WorkspaceId);
        });

        modelBuilder.Entity<KanbanCard>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(250);
            entity.Property(e => e.Column).IsRequired().HasMaxLength(30);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.HasIndex(e => new { e.WorkspaceId, e.Column });
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(150);
            entity.Property(e => e.CreatedAt).HasPrecision(0);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.HasIndex(e => e.WorkspaceId);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BlogCategory>().HasData(
            new BlogCategory { Id = 1, Name = "Productivity", Slug = "productivity" },
            new BlogCategory { Id = 2, Name = "Business", Slug = "business" },
            new BlogCategory { Id = 3, Name = "Education", Slug = "education" },
            new BlogCategory { Id = 4, Name = "Technology", Slug = "technology" },
            new BlogCategory { Id = 5, Name = "Finance", Slug = "finance" }
        );
    }
}
