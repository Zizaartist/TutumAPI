using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using TutumAPI.Models;

#nullable disable

namespace TutumAPI
{
    public partial class DatabaseContext : DbContext
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Course> Courses { get; set; }
        public virtual DbSet<Lesson> Lessons { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "SQL_Latin1_General_CP1_CS_AS");

            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Course");

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.PreviewPath)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);
            });

            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.ToTable("Lesson");

                entity.Property(e => e.PreviewPath)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Text)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.VideoPath)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.Course)
                    .WithMany(p => p.Lessons)
                    .HasForeignKey(d => d.CourseId)
                    .HasConstraintName("FK_Lesson_Course");
            });

            modelBuilder.Entity<Subscription>(entity =>
            {
                entity.ToTable("Subscription");

                entity.HasIndex(e => e.UserId, "IX_Subscription");

                entity.Property(e => e.ActivationDate).HasColumnType("date");

                entity.Property(e => e.Expires).HasColumnType("date");

                entity.HasOne(d => d.User)
                    .WithOne(p => p.Subscription)
                    .HasForeignKey<Subscription>(d => d.UserId)
                    .HasConstraintName("FK_Subscription_User");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Phone, "IX_User")
                    .IsUnique();

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
