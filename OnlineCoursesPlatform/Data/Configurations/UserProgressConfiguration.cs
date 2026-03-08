using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
    {
        public void Configure(EntityTypeBuilder<UserProgress> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(up => up.Id);

            // 2. إعدادات الخصائص
            builder.Property(up => up.IsCompleted)
                .HasDefaultValue(false); // الحالة الافتراضية "لم يتم الإكمال"

            // 3. العلاقات (Relationships)
            // ربط الطالب بتقدمه في الدروس
            builder.HasOne(up => up.Student)
                .WithMany(u => u.Progresses)
                .HasForeignKey(up => up.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ربط الدرس بسجل التقدم
            builder.HasOne(up => up.Lesson)
                .WithMany(l => l.UserProgresses)
                .HasForeignKey(up => up.LessonId)
                .OnDelete(DeleteBehavior.Cascade); // لو الدرس اتمسح، سجل تقدم الطلاب فيه ملوش لازمة

            // 4. منع التكرار (Unique Index)
            // الطالب يكون عنده سجل واحد فقط لكل درس (عشان ميعملش "Complete" لنفس الدرس مرتين)
            builder.HasIndex(up => new { up.StudentId, up.LessonId }).IsUnique();
        }
    }
}