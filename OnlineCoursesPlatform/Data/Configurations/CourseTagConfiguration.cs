using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

public class CourseTagConfiguration : IEntityTypeConfiguration<CourseTag>
{
    public void Configure(EntityTypeBuilder<CourseTag> builder)
    {
        // السطر ده هو أهم سطر ومن غيره الـ Migration هتفشل
        builder.HasKey(ct => new { ct.CourseId, ct.TagId });

        // ربط جهة الكورس
        builder.HasOne(ct => ct.Course)
               .WithMany(c => c.CourseTags)
               .HasForeignKey(ct => ct.CourseId);

        // ربط جهة التاج
        builder.HasOne(ct => ct.Tag)
               .WithMany(t => t.CourseTags)
               .HasForeignKey(ct => ct.TagId);
    }
}