using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Dtos;
using OnlineCoursesPlatform.Infrastructure;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;

namespace OnlineCoursesPlatform.Services
{
    public class CourseService : ICourseService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IMapper _mapper;
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CourseService(ICourseRepository courseRepository, IMapper mapper, AppDbContext context, IWebHostEnvironment environment)
        {
            _courseRepository = courseRepository;
            _mapper = mapper;
            _context = context;
            _environment = environment;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesAsync()
        {
            return await _courseRepository.GetAllAsync();
        }

        public async Task<Course> GetCourseByIdAsync(int id)
        {
            return await _courseRepository.GetByIdAsync(id);
        }

        public async Task<Course> GetCourseWithDetailsAsync(int id)
        {
            return await _courseRepository.GetCourseWithDetailsAsync(id);
        }

        public async Task<CourseDetailsViewModel> GetCourseDetailsProjectedAsync(int id)
        {
            return await _courseRepository.GetQueryable()
                .Where(c => c.Id == id)
                .ProjectTo<CourseDetailsViewModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
        }

        public async Task<CourseEditorResultDto> GetCourseForEditAsync(int courseId, int actingUserId, bool isAdmin)
        {
            var course = await _context.Courses
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == courseId);

            if (course == null)
            {
                return new CourseEditorResultDto { NotFound = true };
            }

            if (!isAdmin && course.InstructorId != actingUserId)
            {
                return new CourseEditorResultDto { IsForbidden = true };
            }

            return new CourseEditorResultDto
            {
                ViewModel = new EditCourseViewModel
                {
                    Id = course.Id,
                    Title = course.Title,
                    Description = course.Description,
                    Price = course.Price,
                    Level = course.Level,
                    Language = course.Language,
                    ExistingImageUrl = string.IsNullOrWhiteSpace(course.ImageUrl)
                        ? "/images/default-course.jpg"
                        : course.ImageUrl
                }
            };
        }

        public async Task<IEnumerable<Course>> GetCoursesWithDetailsAsync()
        {
            return await _courseRepository.GetCoursesWithDetailsAsync();
        }

        public async Task<IEnumerable<Course>> GetCoursesByInstructorAsync(int instructorId)
        {
            return await _courseRepository.GetCoursesByInstructorAsync(instructorId);
        }

        public async Task<IEnumerable<Course>> GetCoursesByCategoryAsync(int categoryId)
        {
            return await _courseRepository.GetCoursesByCategoryAsync(categoryId);
        }

        public async Task<Course> CreateCourseAsync(Course course)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course));
            }

            if (string.IsNullOrWhiteSpace(course.Title))
            {
                throw new ArgumentException("Course title is required", nameof(course.Title));
            }

            if (string.IsNullOrWhiteSpace(course.Description))
            {
                throw new ArgumentException("Course description is required", nameof(course.Description));
            }

            if (course.Price < 0)
            {
                throw new ArgumentException("Course price cannot be negative", nameof(course.Price));
            }

            if (course.CategoryId <= 0)
            {
                throw new ArgumentException("Valid category is required", nameof(course.CategoryId));
            }

            if (course.CurrencyId <= 0)
            {
                throw new ArgumentException("Valid currency is required", nameof(course.CurrencyId));
            }

            if (course.InstructorId <= 0)
            {
                throw new ArgumentException("Instructor must be assigned", nameof(course.InstructorId));
            }

            if (course.TotalDuration < 0)
            {
                throw new ArgumentException("Course duration cannot be negative", nameof(course.TotalDuration));
            }

            var createdCourse = await _courseRepository.AddAsync(course);
            await _courseRepository.SaveAsync();
            return createdCourse;
        }

        public async Task<CourseUpdateResultDto> UpdateCourseAsync(EditCourseViewModel model, int actingUserId, bool isAdmin)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(item => item.Id == model.Id);
            if (course == null)
            {
                return new CourseUpdateResultDto
                {
                    NotFound = true,
                    Errors = ["Course not found."]
                };
            }

            if (!isAdmin && course.InstructorId != actingUserId)
            {
                return new CourseUpdateResultDto
                {
                    IsForbidden = true
                };
            }

            course.Title = model.Title.Trim();
            course.Description = model.Description.Trim();
            course.Price = model.Price;
            course.Level = model.Level;
            course.Language = model.Language.Trim();

            string? previousImageUrl = null;
            string? newImageUrl = null;

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                previousImageUrl = course.ImageUrl;
                newImageUrl = await SaveCourseImageAsync(model.ImageFile);
                course.ImageUrl = newImageUrl;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(newImageUrl))
                {
                    DeleteManagedCourseImage(newImageUrl);
                }

                return new CourseUpdateResultDto
                {
                    Errors = ["We could not save your changes right now. Please try again."]
                };
            }

            if (!string.IsNullOrWhiteSpace(previousImageUrl))
            {
                DeleteManagedCourseImage(previousImageUrl);
            }

            return new CourseUpdateResultDto
            {
                Succeeded = true,
                Message = "Course information updated successfully."
            };
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.CourseTags)
                .Include(c => c.Enrollments)
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return false;
            }

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

            return true;
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _courseRepository.SaveAsync();
        }

        public async Task<(IEnumerable<CourseListViewModel> Courses, int TotalCount)> GetPaginatedCoursesAsync(int pageNumber, int pageSize)
        {
            var query = _courseRepository.GetQueryable();
            var totalCount = await query.CountAsync();

            var courses = await query
                .OrderByDescending(c => c.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ProjectTo<CourseListViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (courses, totalCount);
        }

        private async Task<string> SaveCourseImageAsync(IFormFile imageFile)
        {
            var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? _environment.WebRootPath
                : Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "images", "courses");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(imageFile.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(fileStream);

            return $"/images/courses/{uniqueFileName}";
        }

        private void DeleteManagedCourseImage(string imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)
                || !imageUrl.StartsWith("/images/courses/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var webRootPath = !string.IsNullOrWhiteSpace(_environment.WebRootPath)
                ? _environment.WebRootPath
                : Path.Combine(_environment.ContentRootPath, "wwwroot");
            var uploadsFolder = Path.Combine(webRootPath, "images", "courses");
            var currentFileName = Path.GetFileName(imageUrl);
            var imagePath = Path.Combine(uploadsFolder, currentFileName);

            if (File.Exists(imagePath))
            {
                File.Delete(imagePath);
            }
        }
    }
}
