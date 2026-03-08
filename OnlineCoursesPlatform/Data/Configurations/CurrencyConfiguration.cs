using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class CurrencyConfiguration : IEntityTypeConfiguration<Currency>
    {
        public void Configure(EntityTypeBuilder<Currency> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(c => c.Id);

            // 2. إعدادات الخصائص
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50); // مثال: Egyptian Pound

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(3); // كود العملة القياسي مثل EGP, USD

            builder.Property(c => c.Symbol)
                .IsRequired()
                .HasMaxLength(5); // الرمز مثل $, £, ج.م

            // 3. العلاقات (Relationships)
            // ربط العملة بالكورسات (One-to-Many)
            builder.HasMany(c => c.Courses)
                .WithOne(course => course.Currency)
                .HasForeignKey(course => course.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict); // منع حذف عملة إذا كانت مرتبطة بكورسات
        }
    }
}