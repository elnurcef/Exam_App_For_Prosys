using ExamManagement.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExamManagement.Api.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Exam> Exams => Set<Exam>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.FirstName).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
            entity.Property(t => t.LastName).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
            entity.Property(t => t.Email).HasColumnType("varchar(100)").HasMaxLength(100).IsRequired();
            entity.Property(t => t.PasswordHash).HasColumnType("varchar(max)").IsRequired();
            entity.Property(t => t.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(t => t.UpdatedAt).HasColumnType("datetime");
            entity.HasIndex(t => t.Email).IsUnique();
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Lessons_ClassLevel", "[ClassLevel] BETWEEN 1 AND 12");
            });
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Code).HasColumnType("char(3)").HasMaxLength(3).IsRequired();
            entity.Property(l => l.Name).HasColumnType("varchar(30)").HasMaxLength(30).IsRequired();
            entity.Property(l => l.ClassLevel).HasColumnType("tinyint").IsRequired();
            entity.Property(l => l.TeacherFirstName).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
            entity.Property(l => l.TeacherLastName).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
            entity.Property(l => l.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(l => l.UpdatedAt).HasColumnType("datetime");
            entity.HasIndex(l => new { l.TeacherId, l.Code }).IsUnique();
            entity.HasOne(l => l.Teacher)
                .WithMany(t => t.Lessons)
                .HasForeignKey(l => l.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Students_Number", "[Number] BETWEEN 1 AND 99999");
                t.HasCheckConstraint("CK_Students_ClassLevel", "[ClassLevel] BETWEEN 1 AND 12");
            });
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Number).IsRequired();
            entity.Property(s => s.FirstName).HasColumnType("varchar(30)").HasMaxLength(30).IsRequired();
            entity.Property(s => s.LastName).HasColumnType("varchar(30)").HasMaxLength(30).IsRequired();
            entity.Property(s => s.ClassLevel).HasColumnType("tinyint").IsRequired();
            entity.Property(s => s.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(s => s.UpdatedAt).HasColumnType("datetime");
            entity.HasIndex(s => new { s.TeacherId, s.Number }).IsUnique();
            entity.HasOne(s => s.Teacher)
                .WithMany(t => t.Students)
                .HasForeignKey(s => s.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Exam>(entity =>
        {
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Exams_Grade", "[Grade] IS NULL OR ([Grade] BETWEEN 0 AND 5)");
                t.HasCheckConstraint("CK_Exams_Status", "[Status] IN ('Scheduled', 'Graded', 'Cancelled')");
            });
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExamDate).HasColumnType("date").IsRequired();
            entity.Property(e => e.Grade).HasColumnType("tinyint");
            entity.Property(e => e.Status).HasColumnType("varchar(20)").HasMaxLength(20).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime").IsRequired();
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.HasIndex(e => new { e.TeacherId, e.LessonId, e.StudentId, e.ExamDate })
                .IsUnique()
                .HasFilter("[Status] <> 'Cancelled'");

            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.Exams)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Lesson)
                .WithMany(l => l.Exams)
                .HasForeignKey(e => e.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Student)
                .WithMany(s => s.Exams)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
