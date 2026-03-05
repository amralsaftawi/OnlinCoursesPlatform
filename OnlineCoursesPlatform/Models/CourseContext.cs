using Microsoft.EntityFrameworkCore;

namespace OnlineCoursesPlatform.Models
{
    public class CourseContext : DbContext
    {
        public CourseContext(DbContextOptions<CourseContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }

        public DbSet<Section> Sections { get; set; }

        public DbSet<Lesson> Lessons { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<CourseTag> CourseTags { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }

        public DbSet<UserProgress> UserProgresses { get; set; }

        public DbSet<Review> Reviews { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Key
            modelBuilder.Entity<CourseTag>()
                .HasKey(ct => new { ct.CourseId, ct.TagId });

            // Unique Indexes
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();

            modelBuilder.Entity<UserProgress>()
                .HasIndex(up => new { up.StudentId, up.LessonId })
                .IsUnique();

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.StudentId, r.CourseId })
                .IsUnique();

            modelBuilder.Entity<Section>()
                .HasIndex(s => new { s.CourseId, s.OrderIndex })
                .IsUnique();

            modelBuilder.Entity<Lesson>()
                .HasIndex(l => new { l.SectionId, l.OrderIndex })
                .IsUnique();

            // Currency Code Unique
            modelBuilder.Entity<Currency>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Cascade Delete
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}