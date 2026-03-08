using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(r => r.Id);

            // 2. إعدادات الخصائص
            builder.Property(r => r.Rating)
                .IsRequired(); // التقييم (النجوم) إلزامي

            builder.Property(r => r.Comment)
                .HasMaxLength(1000); // تحديد سقف للتعليق عشان الداتا بيز متتقلش

            builder.Property(r => r.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()"); // تسجيل وقت التقييم آلياً

            // 3. العلاقات (Relationships)
            // ربط الطالب بالتقييم
            builder.HasOne(r => r.Student)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ربط الكورس بالتقييم
            builder.HasOne(r => r.Course)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade); // لو الكورس اتمسح، تقييماته تتمسح معاه

            // 4. قاعدة البزنس: الطالب يقيم الكورس مرة واحدة فقط
            builder.HasIndex(r => new { r.StudentId, r.CourseId }).IsUnique();
        }
    }
}