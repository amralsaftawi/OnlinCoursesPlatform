using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class TagConfiguration : IEntityTypeConfiguration<Tag>
    {
        public void Configure(EntityTypeBuilder<Tag> builder)
        {
            // 1. المفتاح الأساسي
            builder.HasKey(t => t.Id);

            // 2. إعدادات الخصائص
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(50); // الوسوم عادةً تكون كلمات قصيرة

            // تأكد إن اسم الوسم مبيتكررش (Unique)
            builder.HasIndex(t => t.Name).IsUnique(); 
        }
    }
}