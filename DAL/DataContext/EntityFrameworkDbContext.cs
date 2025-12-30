using Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.DataContext
{
    public class EntityFrameworkDbContext : DbContext
    {
        public EntityFrameworkDbContext(DbContextOptions<EntityFrameworkDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee configuration
            modelBuilder.Entity<Employee>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Department).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
                entity.Property(e => e.HireDate).IsRequired();
                entity.Property(e => e.Phone).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(200);

                // FIX: Add precision for decimal property
                entity.Property(e => e.Salary).HasPrecision(18, 2); // 18 total digits, 2 decimal places

                entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(e => e.UpdatedAt).IsRequired(false);
            });

            // Project configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Description).HasMaxLength(500);
                entity.Property(p => p.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Active");
                entity.Property(p => p.Technology).HasMaxLength(50);
                entity.Property(p => p.ClientName).HasMaxLength(100);

                // FIX: Add precision for decimal property
                entity.Property(p => p.Budget).HasPrecision(18, 2); // 18 total digits, 2 decimal places

                entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(p => p.UpdatedAt).IsRequired(false);
            });

            // Ticket configuration
            modelBuilder.Entity<Ticket>(entity =>
            {
                entity.HasKey(t => t.Id);
                entity.Property(t => t.Title).IsRequired().HasMaxLength(200);
                entity.Property(t => t.Description).HasMaxLength(1000);
                entity.Property(t => t.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Open");
                entity.Property(t => t.Priority).IsRequired().HasMaxLength(20).HasDefaultValue("Medium");
                entity.Property(t => t.Resolution).HasMaxLength(500);
                entity.Property(t => t.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
                entity.Property(t => t.UpdatedAt).IsRequired(false);
                entity.Property(t => t.DueDate).IsRequired(false);
                entity.Property(t => t.ResolvedDate).IsRequired(false);
                entity.Property(t => t.AssignedTo).IsRequired(false);

                // Relationships
                entity.HasOne(t => t.Employee)
                    .WithMany(e => e.Tickets)
                    .HasForeignKey(t => t.AssignedTo)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(t => t.Project)
                    .WithMany(p => p.Tickets)
                    .HasForeignKey(t => t.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}