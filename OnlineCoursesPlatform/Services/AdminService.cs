using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Services.Interfaces;

namespace OnlineCoursesPlatform.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public AdminService(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            return new AdminDashboardDto
            {
                UsersCount = await _context.Users.CountAsync(),

                CoursesCount = await _context.Courses.CountAsync(),

                PendingCoursesCount = await _context.Courses
                    .CountAsync(c => c.Status == CourseStatus.Pending),

                LatestUsers = await _context.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync(),

                PendingCourses = await _context.Courses
                    .Where(c => c.Status == CourseStatus.Pending)
                    .Include(c => c.Instructor)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task<IEnumerable<AdminUserDto>> GetUsersAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .ToListAsync();

            var result = new List<AdminUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new AdminUserDto
                {
                    Id = user.Id,
                    Name = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Roles = (List<string>)roles
                });
            }

            return result;
        }

        public async Task MakeAdminAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                throw new Exception("User not found");

            // لو هو already admin متعملش حاجة
            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return;

            // Remove old roles (اختياري حسب السيستم بتاعك)
            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
                await _userManager.RemoveFromRolesAsync(user, currentRoles);

            await _userManager.AddToRoleAsync(user, "Admin");
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
                return;

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));
        }

        public async Task<IEnumerable<AdminCourseDto>> GetCoursesAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Select(c => new AdminCourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    InstructorName = c.Instructor.FirstName + " " + c.Instructor.LastName,
                    Status = c.Status.ToString()
                })
                .ToListAsync();
        }

        public async Task ApproveCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
                throw new Exception("Course not found");

            if (course.Status == CourseStatus.Approved)
                return;

            course.Status = CourseStatus.Approved;
            await _context.SaveChangesAsync();
        }

        public async Task RejectCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null)
                throw new Exception("Course not found");

            if (course.Status == CourseStatus.Rejected)
                return;

            course.Status = CourseStatus.Rejected;
            await _context.SaveChangesAsync();
        }
    }
}