using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
    {
        public void Configure(EntityTypeBuilder<Enrollment> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(e => e.Id);

            // 2. إعدادات الخصائص
            builder.Property(e => e.ProgressPercentage)
                .HasPrecision(5, 2) // يسمح بنسبة مثل 100.00%
                .HasDefaultValue(0.00m);

            builder.Property(e => e.EnrolledAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()"); // تسجيل وقت الاشتراك تلقائياً من السيرفر

            // 3. العلاقات (Relationships)
            // ربط الطالب بالاشتراك
            builder.HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict); // لا نحذف الاشتراك لمجرد حذف بيانات تقنية للمستخدم

            // ربط الكورس بالاشتراك
            builder.HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. منع التكرار (Unique Index)
            // الطالب مينفعش يشترك في نفس الكورس مرتين
            builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        }
    }
}