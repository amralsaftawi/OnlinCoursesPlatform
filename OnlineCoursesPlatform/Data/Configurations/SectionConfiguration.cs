using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class SectionConfiguration : IEntityTypeConfiguration<Section>
    {
        public void Configure(EntityTypeBuilder<Section> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(s => s.Id);

            // 2. إعدادات الخصائص
            builder.Property(s => s.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.OrderIndex)
                .IsRequired();

            // 3. العلاقات (Relationships)
            // ربط السكشن بالكورس (One-to-Many)
            builder.HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // إذا حُذف الكورس، تُحذف جميع سكاشنه تلقائياً

            // ربط السكشن بالدروس (One-to-Many)
            builder.HasMany(s => s.Lessons)
                .WithOne(l => l.Section)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade); // إذا حُذف السكشن، تُحذف دروسه تلقائياً
        }
    }
}