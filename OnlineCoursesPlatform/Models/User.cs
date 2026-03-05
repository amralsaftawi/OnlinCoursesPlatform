using Microsoft.AspNetCore.Mvc.ViewEngines;
using OnlineCoursesPlatform.Models.Enums;

namespace OnlineCoursesPlatform.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public UserRole Role { get; set; }

        public ICollection<Course> Courses { get; set; }

        public ICollection<Enrollment> Enrollments { get; set; }

        public ICollection<UserProgress> Progresses { get; set; }

        public ICollection<Review> Reviews { get; set; }
    }
}
