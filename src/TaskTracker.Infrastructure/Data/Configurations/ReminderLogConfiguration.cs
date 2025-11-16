using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Data.Configurations;

public class ReminderLogConfiguration : IEntityTypeConfiguration<ReminderLog>
{
    public void Configure(EntityTypeBuilder<ReminderLog> builder)
    {
        builder.ToTable("reminder_logs");

        builder.HasKey(rl => rl.Id);
        
        builder.Property(rl => rl.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(rl => rl.TaskId)
            .HasColumnName("task_id")
            .IsRequired();

        builder.Property(rl => rl.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rl => rl.ReminderSentAt)
            .HasColumnName("reminder_sent_at")
            .IsRequired();

        builder.Property(rl => rl.TaskDueDate)
            .HasColumnName("task_due_date")
            .IsRequired();

        builder.Property(rl => rl.ReminderType)
            .HasColumnName("reminder_type")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(rl => rl.DeliverySuccessful)
            .HasColumnName("delivery_successful")
            .IsRequired();

        builder.Property(rl => rl.DeliveryDetails)
            .HasColumnName("delivery_details")
            .HasMaxLength(500);

        builder.Property(rl => rl.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationships
        builder.HasOne(rl => rl.Task)
            .WithMany()
            .HasForeignKey(rl => rl.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rl => rl.User)
            .WithMany()
            .HasForeignKey(rl => rl.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique constraint for idempotency - prevent duplicate reminders
        builder.HasIndex(rl => new { rl.TaskId, rl.ReminderType, rl.TaskDueDate })
            .IsUnique()
            .HasDatabaseName("ix_reminder_logs_unique_reminder");

        // Additional indexes
        builder.HasIndex(rl => rl.TaskId)
            .HasDatabaseName("ix_reminder_logs_task_id");

        builder.HasIndex(rl => rl.UserId)
            .HasDatabaseName("ix_reminder_logs_user_id");

        builder.HasIndex(rl => rl.ReminderSentAt)
            .HasDatabaseName("ix_reminder_logs_reminder_sent_at");
    }
}