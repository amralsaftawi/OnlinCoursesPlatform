using global::OnlineCoursesPlatform.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(c => c.Id);

            // 2. إعدادات الخصائص (Validation & Schema)
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(c => c.Description)
                .IsRequired();

            builder.Property(c => c.Price)
                .HasPrecision(18, 2); // تحديد الدقة للعملات

            builder.Property(c => c.ImageUrl)
                .HasMaxLength(500);

            builder.Property(c => c.Language)
                .HasMaxLength(50);

            // 3. التعامل مع الـ Enums (تخزينها كـ Strings أو Integers)
            builder.Property(c => c.Status)
                .HasConversion<string>(); // بيخليها تظهر في الداتا بيز ككلمة (Draft, Published) بدل أرقام

            builder.Property(c => c.Level)
                .HasConversion<string>();

            // 4. العلاقات (Relationships) بناءً على الـ ERD
            builder.HasOne(c => c.Currency)
                .WithMany()
                .HasForeignKey(c => c.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Instructor)
                .WithMany()
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Category)
                .WithMany(cat => cat.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}