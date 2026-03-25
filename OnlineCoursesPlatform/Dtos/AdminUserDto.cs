namespace OnlineCoursesPlatform.Dtos
{
    public class AdminUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public string PrimaryRole { get; set; } = "Student";
        public bool IsAdmin { get; set; }
    }
}
