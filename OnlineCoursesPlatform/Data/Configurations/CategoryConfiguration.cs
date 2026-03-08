using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(c => c.Id);

            // 2. إعدادات الخصائص
            builder.Property(c => c.Title)
                .IsRequired()
                .HasMaxLength(100); // عناوين التصنيفات غالباً ما تكون قصيرة ومباشرة

            // 3. العلاقات (Relationships)
            // علاقة One-to-Many مع الكورسات
            builder.HasMany(c => c.Courses)
                .WithOne(course => course.Category)
                .HasForeignKey(course => course.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // منع حذف التصنيف إذا كان يحتوي على كورسات
        }
    }
}