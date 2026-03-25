using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineCoursesPlatform.Models;

namespace OnlineCoursesPlatform.Data.Configurations
{
    public class AdminConfiguration : IEntityTypeConfiguration<AdminProfile>
    {
        public void Configure(EntityTypeBuilder<AdminProfile> builder)
        {
            builder.HasKey(a => a.Id);

            builder.HasOne(a => a.ApplicationUser)
                   .WithOne(u => u.AdminProfile)
                   .HasForeignKey<AdminProfile>(a => a.ApplicationUserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.ApplicationUserId).IsUnique();
        }
    }
}