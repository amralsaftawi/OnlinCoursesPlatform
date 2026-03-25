namespace OnlineCoursesPlatform.Dtos
{
    public class ProfileDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string? ProfilePicture { get; set; }

        public IList<string> Roles { get; set; } = new List<string>();

        // Helpers (تسهل عليك في الـ View)
        public bool IsInstructor => Roles.Contains("Instructor");
        public bool IsAdmin => Roles.Contains("Admin");
        public bool IsStudent => Roles.Contains("Student");
    }
}
