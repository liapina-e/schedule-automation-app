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

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TargetGrade).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.OwnsMany(e => e.Components, component =>
            {
                component.WithOwner().HasForeignKey("SubjectId");
                component.HasKey(c => c.Id);

                component.Property(c => c.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                component.OwnsOne(c => c.Weight, w =>
                {
                    w.Property(p => p.Value)
                        .HasColumnName("Weight")
                        .HasPrecision(5, 2)
                        .IsRequired();
                });

                component.OwnsOne(c => c.Complexity, c =>
                {
                    c.Property(p => p.Value)
                        .HasColumnName("Complexity")
                        .IsRequired();
                });

                component.OwnsOne(c => c.CurrentGrade, g =>
                {
                    g.Property(p => p.Value)
                        .HasColumnName("CurrentGrade")
                        .HasPrecision(4, 2)
                        .IsRequired();
                });

                component.Property(c => c.IsBlocking).HasDefaultValue(false);
                component.Property(c => c.MinimumGrade)
                    .HasPrecision(4, 2)
                    .HasDefaultValue(0);
            });

            entity.HasIndex(e => e.Name);
        });
    }
}
