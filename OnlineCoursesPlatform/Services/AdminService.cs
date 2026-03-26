using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Infrastructure;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Services.Interfaces;

namespace OnlineCoursesPlatform.Services
{
    public class AdminService : IAdminService
    {
        private static readonly string[] ManagedRoles = ["Student", "Instructor", "Admin"];

        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AdminService(AppDbContext context, UserManager<User> userManager, IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var revenueByCurrency = await _context.Enrollments
                .AsNoTracking()
                .GroupBy(enrollment => new { enrollment.Course.Currency.Code, enrollment.Course.Currency.Symbol })
                .Select(group => new AdminRevenueDto
                {
                    CurrencyCode = group.Key.Code,
                    CurrencySymbol = group.Key.Symbol,
                    TotalRevenue = group.Sum(enrollment => enrollment.Course.Price)
                })
                .OrderByDescending(item => item.TotalRevenue)
                .ToListAsync();

            return new AdminDashboardDto
            {
                UsersCount = await _context.Users.CountAsync(),
                CoursesCount = await _context.Courses.CountAsync(),
                PendingCoursesCount = await _context.Courses.CountAsync(c => c.Status == CourseStatus.Pending),
                ApprovedCoursesCount = await _context.Courses.CountAsync(c => c.Status == CourseStatus.Approved),
                RejectedCoursesCount = await _context.Courses.CountAsync(c => c.Status == CourseStatus.Rejected),
                CategoriesCount = await _context.Categories.CountAsync(),
                CurrenciesCount = await _context.Currencies.CountAsync(),
                TagsCount = await _context.Tags.CountAsync(),
                TotalRevenue = revenueByCurrency.Sum(item => item.TotalRevenue),
                RevenueByCurrency = revenueByCurrency,
                LatestUsers = await _context.Users
                    .OrderByDescending(u => u.Id)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync(),
                PendingCourses = await _context.Courses
                    .Where(c => c.Status == CourseStatus.Pending)
                    .Include(c => c.Instructor)
                    .OrderByDescending(c => c.Id)
                    .Take(5)
                    .AsNoTracking()
                    .ToListAsync()
            };
        }

        public async Task<IEnumerable<AdminUserDto>> GetUsersAsync()
        {
            var users = await _context.Users
                .AsNoTracking()
                .OrderBy(u => u.FirstName)
                .ThenBy(u => u.LastName)
                .ToListAsync();

            var result = new List<AdminUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var primaryRole = roles.Contains("Instructor") ? "Instructor" : "Student";

                result.Add(new AdminUserDto
                {
                    Id = user.Id,
                    Name = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email ?? string.Empty,
                    Roles = roles.OrderBy(role => role).ToList(),
                    PrimaryRole = primaryRole,
                    IsAdmin = roles.Contains("Admin")
                });
            }

            return result;
        }

        public async Task UpdateUserRolesAsync(int userId, string primaryRole, bool isAdmin, int actingAdminId)
        {
            if (!ManagedRoles.Contains(primaryRole))
                throw new Exception("Invalid role selection.");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("User not found.");

            if (user.Id == actingAdminId && !isAdmin)
                throw new Exception("You cannot remove your own admin role.");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var rolesToRemove = currentRoles.Where(role => role is "Instructor" or "Admin").ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                    throw new Exception(string.Join(", ", removeResult.Errors.Select(error => error.Description)));
            }

            var rolesToAdd = new List<string> { "Student" };
            if (string.Equals(primaryRole, "Instructor", StringComparison.OrdinalIgnoreCase))
            {
                rolesToAdd.Add("Instructor");
            }
            if (isAdmin)
            {
                rolesToAdd.Add("Admin");
            }

            var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd.Distinct());
            if (!addResult.Succeeded)
                throw new Exception(string.Join(", ", addResult.Errors.Select(error => error.Description)));
        }

        public async Task DeleteUserAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());

            if (user == null)
            {
                return;
            }

            var ownedCourseIds = await _context.Courses
                .Where(course => course.InstructorId == userId)
                .Select(course => course.Id)
                .ToListAsync();

            foreach (var courseId in ownedCourseIds)
            {
                await DeleteCourseAsync(courseId);
            }

            var userProgresses = await _context.UserProgresses
                .Where(progress => progress.StudentId == userId)
                .ToListAsync();
            if (userProgresses.Count != 0)
            {
                _context.UserProgresses.RemoveRange(userProgresses);
            }

            var userEnrollments = await _context.Enrollments
                .Where(enrollment => enrollment.StudentId == userId)
                .ToListAsync();
            if (userEnrollments.Count != 0)
            {
                _context.Enrollments.RemoveRange(userEnrollments);
            }

            var userReviews = await _context.Reviews
                .Where(review => review.StudentId == userId)
                .ToListAsync();
            if (userReviews.Count != 0)
            {
                _context.Reviews.RemoveRange(userReviews);
            }

            await _context.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new Exception(string.Join(", ", result.Errors.Select(error => error.Description)));
        }

        public async Task<IEnumerable<AdminCourseDto>> GetCoursesAsync()
        {
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Instructor)
                .Include(c => c.Category)
                .Include(c => c.Currency)
                .OrderBy(c => c.Status)
                .ThenByDescending(c => c.Id)
                .Select(c => new AdminCourseDto
                {
                    Id = c.Id,
                    Title = c.Title,
                    InstructorName = $"{c.Instructor.FirstName} {c.Instructor.LastName}",
                    CategoryName = c.Category.Title,
                    Status = c.Status.ToString(),
                    CurrencySymbol = c.Currency.Symbol,
                    Price = c.Price,
                    SectionCount = c.Sections.Count(),
                    LessonCount = c.Sections.SelectMany(section => section.Lessons).Count(),
                    TotalDuration = c.Sections.SelectMany(section => section.Lessons).Sum(lesson => (int?)lesson.Duration) ?? 0
                })
                .ToListAsync();
        }

        public async Task ApproveCourseAsync(int courseId)
        {
            await UpdateCourseStatusAsync(courseId, CourseStatus.Approved);
        }

        public async Task RejectCourseAsync(int courseId)
        {
            await UpdateCourseStatusAsync(courseId, CourseStatus.Rejected);
        }

        public async Task MoveCourseToPendingAsync(int courseId)
        {
            await UpdateCourseStatusAsync(courseId, CourseStatus.Pending);
        }

        public async Task DeleteCourseAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.CourseTags)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                throw new Exception("Course not found.");

            var articleFilesToDelete = course.Sections
                .SelectMany(section => section.Lessons)
                .Where(lesson => lesson.Type == LessonType.Article && LessonContentStorage.IsLocalArticleUpload(lesson.ContentUrl))
                .Select(lesson => lesson.ContentUrl!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var lessonIds = course.Sections
                .SelectMany(section => section.Lessons)
                .Select(lesson => lesson.Id)
                .ToList();

            if (lessonIds.Count != 0)
            {
                var progresses = await _context.UserProgresses
                    .Where(progress => lessonIds.Contains(progress.LessonId))
                    .ToListAsync();

                if (progresses.Count != 0)
                {
                    _context.UserProgresses.RemoveRange(progresses);
                }
            }

            if (course.Reviews.Any())
            {
                _context.Reviews.RemoveRange(course.Reviews);
            }

            if (course.Enrollments.Any())
            {
                _context.Enrollments.RemoveRange(course.Enrollments);
            }

            if (course.CourseTags.Any())
            {
                _context.CourseTags.RemoveRange(course.CourseTags);
            }

            var lessons = course.Sections.SelectMany(section => section.Lessons).ToList();
            if (lessons.Count != 0)
            {
                _context.Lessons.RemoveRange(lessons);
            }

            if (course.Sections.Any())
            {
                _context.Sections.RemoveRange(course.Sections);
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            foreach (var articleFile in articleFilesToDelete)
            {
                LessonContentStorage.DeleteLocalArticleFile(_environment, articleFile);
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Title)
                .ToListAsync();
        }

        public async Task<IEnumerable<Currency>> GetCurrenciesAsync()
        {
            return await _context.Currencies
                .AsNoTracking()
                .OrderBy(c => c.Code)
                .ToListAsync();
        }

        public async Task<IEnumerable<Tag>> GetTagsAsync()
        {
            return await _context.Tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task AddCategoryAsync(string title)
        {
            title = NormalizeValue(title);

            if (await _context.Categories.AnyAsync(c => c.Title.ToLower() == title.ToLower()))
                throw new Exception("Category already exists.");

            _context.Categories.Add(new Category { Title = title });
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategoryAsync(int categoryId, string title)
        {
            title = NormalizeValue(title);

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
            if (category == null)
                throw new Exception("Category not found.");

            if (await _context.Categories.AnyAsync(c => c.Id != categoryId && c.Title.ToLower() == title.ToLower()))
                throw new Exception("Category already exists.");

            category.Title = title;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            var category = await _context.Categories.Include(c => c.Courses).FirstOrDefaultAsync(c => c.Id == categoryId);

            if (category == null)
                throw new Exception("Category not found.");

            if (category.Courses.Any())
                throw new Exception("Cannot delete a category that is assigned to courses.");

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }

        public async Task AddCurrencyAsync(string name, string code, string symbol)
        {
            name = NormalizeValue(name);
            code = NormalizeValue(code).ToUpperInvariant();
            symbol = NormalizeValue(symbol);

            if (await _context.Currencies.AnyAsync(c => c.Code == code))
                throw new Exception("Currency code already exists.");

            _context.Currencies.Add(new Currency
            {
                Name = name,
                Code = code,
                Symbol = symbol
            });
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCurrencyAsync(int currencyId, string name, string code, string symbol)
        {
            name = NormalizeValue(name);
            code = NormalizeValue(code).ToUpperInvariant();
            symbol = NormalizeValue(symbol);

            var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Id == currencyId);
            if (currency == null)
                throw new Exception("Currency not found.");

            if (await _context.Currencies.AnyAsync(c => c.Id != currencyId && c.Code == code))
                throw new Exception("Currency code already exists.");

            currency.Name = name;
            currency.Code = code;
            currency.Symbol = symbol;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCurrencyAsync(int currencyId)
        {
            var currency = await _context.Currencies.Include(c => c.Courses).FirstOrDefaultAsync(c => c.Id == currencyId);

            if (currency == null)
                throw new Exception("Currency not found.");

            if (currency.Courses.Any())
                throw new Exception("Cannot delete a currency that is used by courses.");

            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();
        }

        public async Task AddTagAsync(string name)
        {
            name = NormalizeValue(name);

            if (await _context.Tags.AnyAsync(t => t.Name.ToLower() == name.ToLower()))
                throw new Exception("Tag already exists.");

            _context.Tags.Add(new Tag { Name = name });
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTagAsync(int tagId, string name)
        {
            name = NormalizeValue(name);

            var tag = await _context.Tags.FirstOrDefaultAsync(t => t.Id == tagId);
            if (tag == null)
                throw new Exception("Tag not found.");

            if (await _context.Tags.AnyAsync(t => t.Id != tagId && t.Name.ToLower() == name.ToLower()))
                throw new Exception("Tag already exists.");

            tag.Name = name;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTagAsync(int tagId)
        {
            var tag = await _context.Tags
                .Include(t => t.CourseTags)
                .FirstOrDefaultAsync(t => t.Id == tagId);

            if (tag == null)
                throw new Exception("Tag not found.");

            if (tag.CourseTags.Any())
            {
                _context.CourseTags.RemoveRange(tag.CourseTags);
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }

        private async Task UpdateCourseStatusAsync(int courseId, CourseStatus status)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null)
                throw new Exception("Course not found.");

            course.Status = status;
            await _context.SaveChangesAsync();
        }

        private static string NormalizeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new Exception("A value is required.");

            return value.Trim();
        }
    }
}
