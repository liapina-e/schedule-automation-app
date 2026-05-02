using Microsoft.EntityFrameworkCore;
using schedule_automation_app_server.Domain.Entities;

namespace schedule_automation_app_server.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public DbSet<Subject> Subjects { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TargetGrade).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();
            entity.HasIndex(e => e.Name);

            entity.HasMany(e => e.Components)
                .WithOne()
                .HasForeignKey(c => c.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<GradeComponent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Weight).HasPrecision(5, 2).IsRequired();
            entity.Property(e => e.Complexity).IsRequired();
            entity.Property(e => e.CurrentGrade).HasPrecision(4, 2).IsRequired();
            entity.Property(e => e.IsBlocking).HasDefaultValue(false);
            entity.Property(e => e.MinimumGrade).HasPrecision(4, 2).HasDefaultValue(0.0);
            entity.Property(e => e.IsGraded).HasDefaultValue(false);
            entity.Property(e => e.IsAutoGrade).HasDefaultValue(false);
            entity.Property(e => e.AutoGradeMinScore).HasPrecision(4, 2).HasDefaultValue(0.0);
        });
    }
}