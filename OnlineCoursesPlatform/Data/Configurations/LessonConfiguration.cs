using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
    {
        public void Configure(EntityTypeBuilder<Lesson> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(l => l.Id);

            // 2. إعدادات الخصائص
            builder.Property(l => l.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(l => l.ContentUrl)
                .IsRequired()
                .HasMaxLength(1000); // روابط الفيديو أو الملفات محتاجة مساحة أكبر

            builder.Property(l => l.Duration)
                .IsRequired();

            builder.Property(l => l.OrderIndex)
                .IsRequired();

            // 3. التعامل مع الـ Enums
            builder.Property(l => l.Type)
                .HasConversion<string>(); // (Video, Quiz, File, Text)

            // 4. القيم الافتراضية
            builder.Property(l => l.IsFree)
                .HasDefaultValue(false); // الدروس مدفوعة بشكل افتراضي إلا لو المدرس حدد

            // 5. العلاقات (Relationships)
            builder.HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة التقدم (Many-to-Many through UserProgress)
            builder.HasMany(l => l.UserProgresses)
                .WithOne(up => up.Lesson)
                .HasForeignKey(up => up.LessonId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}