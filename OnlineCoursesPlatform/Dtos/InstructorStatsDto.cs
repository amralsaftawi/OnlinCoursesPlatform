namespace OnlinCoursePlatform.Dtos;


public class InstructorStatsDto
{
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; } // إجمالي الأرباح
    public double AverageRating { get; set; } // متوسط التقييم لكورساته
    
    // ممكن تضيف "أحدث الطلاب" كـ List لو حبيت
    public List<RecentEnrollmentDto> RecentEnrollments { get; set; } = new();
}


public class RecentEnrollmentDto
{
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
}