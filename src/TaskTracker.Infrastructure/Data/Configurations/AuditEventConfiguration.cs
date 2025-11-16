using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Infrastructure.Data.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");

        builder.HasKey(ae => ae.Id);
        
        builder.Property(ae => ae.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        builder.Property(ae => ae.Action)
            .HasColumnName("action")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(ae => ae.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ae => ae.EntityId)
            .HasColumnName("entity_id");

        builder.Property(ae => ae.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100);

        builder.Property(ae => ae.Details)
            .HasColumnName("details")
            .HasMaxLength(1000);

        builder.Property(ae => ae.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Relationships
        builder.HasOne(ae => ae.User)
            .WithMany()
            .HasForeignKey(ae => ae.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(ae => ae.UserId)
            .HasDatabaseName("ix_audit_events_user_id");

        builder.HasIndex(ae => ae.CreatedAt)
            .HasDatabaseName("ix_audit_events_created_at");

        builder.HasIndex(ae => new { ae.EntityType, ae.EntityId })
            .HasDatabaseName("ix_audit_events_entity");

        builder.HasIndex(ae => ae.Action)
            .HasDatabaseName("ix_audit_events_action");
    }
}