using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Enums;
using System.Text.Json;

namespace TaskTracker.Infrastructure.Data.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<Domain.Entities.TaskItem>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.TaskItem> builder)
    {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(2000);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.DueDate)
            .HasColumnName("due_date");

        builder.Property(t => t.Tags)
            .HasColumnName("tags")
            .HasConversion(
                tags => JsonSerializer.Serialize(tags, (JsonSerializerOptions?)null),
                json => JsonSerializer.Deserialize<ICollection<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>()
            )
            .HasColumnType("jsonb");

        builder.Property(t => t.OwnerUserId)
            .HasColumnName("owner_user_id")
            .IsRequired();

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Property(t => t.CompletedAt)
            .HasColumnName("completed_at");

        // Relationships
        builder.HasOne(t => t.Owner)
            .WithMany()
            .HasForeignKey(t => t.OwnerUserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(t => t.Attachments)
            .WithOne(a => a.Task)
            .HasForeignKey(a => a.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(t => t.OwnerUserId)
            .HasDatabaseName("ix_tasks_owner_user_id");

        builder.HasIndex(t => t.Status)
            .HasDatabaseName("ix_tasks_status");

        builder.HasIndex(t => t.DueDate)
            .HasDatabaseName("ix_tasks_due_date");

        builder.HasIndex(t => new { t.OwnerUserId, t.Status })
            .HasDatabaseName("ix_tasks_owner_status");
    }
}