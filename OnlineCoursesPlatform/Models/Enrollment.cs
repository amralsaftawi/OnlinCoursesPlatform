namespace OnlineCoursesPlatform.Models
{
    public class Enrollment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public User Student { get; set; }

        public int CourseId { get; set; }
        public Course Course { get; set; }

        public decimal ProgressPercentage { get; set; }

        public DateTime EnrolledAt { get; set; }
    }
}
