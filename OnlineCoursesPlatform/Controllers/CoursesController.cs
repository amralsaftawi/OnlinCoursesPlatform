using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Repositories.Interfaces;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace OnlineCoursesPlatform.Controllers
{
    public class CoursesController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IRepository<Currency> _currencyRepo;
        private readonly IRepository<Tag> _tagRepo;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILearningService _learningService;

        public CoursesController(
            ICourseService courseService,
            AppDbContext context,
            IMapper mapper,
            IRepository<Category> categoryRepo,
            IRepository<Currency> currencyRepo,
            IRepository<Tag> tagRepo,
            IWebHostEnvironment webHostEnvironment,
            ILearningService learningService)
        {
            _courseService = courseService;
            _context = context;
            _mapper = mapper;
            _categoryRepo = categoryRepo;
            _currencyRepo = currencyRepo;
            _tagRepo = tagRepo;
            _webHostEnvironment = webHostEnvironment;
            _learningService = learningService;
        }

        [HttpGet]
        public async Task<ActionResult> Index(string searchTerm, int? categoryId, List<int> selectedTags, int page = 1)
        {
            ViewBag.Categories = new SelectList(Enumerable.Empty<Category>(), "Id", "Title");
            ViewBag.Tags = Enumerable.Empty<Tag>();
            ViewBag.SelectedTags = selectedTags ?? new List<int>();
            ViewBag.CurrentSearch = searchTerm;

            try
            {
                const int pageSize = 6;
                var coursesQuery = _context.Courses
                    .AsNoTracking()
                    .Include(c => c.Category)
                    .Include(c => c.Currency)
                    .Include(c => c.Instructor)
                    .Include(c => c.CourseTags)
                    .AsQueryable();

                if (!User.IsInRole("Admin"))
                {
                    coursesQuery = coursesQuery.Where(c => c.Status == CourseStatus.Approved);
                }

                var categories = await _categoryRepo.GetAllAsync() ?? Enumerable.Empty<Category>();
                var tags = await _tagRepo.GetAllAsync() ?? Enumerable.Empty<Tag>();

                ViewBag.Categories = new SelectList(categories, "Id", "Title", categoryId);
                ViewBag.Tags = tags;

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim().ToLower();
                    coursesQuery = coursesQuery.Where(c =>
                        c.Title.ToLower().Contains(term) ||
                        (c.Description != null && c.Description.ToLower().Contains(term)));
                }

                if (categoryId.HasValue && categoryId > 0)
                {
                    coursesQuery = coursesQuery.Where(c => c.CategoryId == categoryId.Value);
                }

                if (selectedTags != null && selectedTags.Any())
                {
                    coursesQuery = coursesQuery.Where(c =>
                        c.CourseTags != null && c.CourseTags.Any(ct => selectedTags.Contains(ct.TagId)));
                }

                page = Math.Max(page, 1);
                var totalCount = await coursesQuery.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                var paginatedCourses = await coursesQuery
                    .OrderByDescending(c => c.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var mappedCourses = _mapper.Map<IEnumerable<CourseListViewModel>>(paginatedCourses);
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.SelectedCategory = categoryId;
                ViewBag.TotalCount = totalCount;
                ViewBag.HasFilters = !string.IsNullOrWhiteSpace(searchTerm) || (categoryId.HasValue && categoryId > 0) || (selectedTags?.Any() == true);

                return View(mappedCourses);
            }
            catch
            {
                ViewBag.Error = "We encountered an issue loading the filters, but you can still browse courses.";
                return View(new List<CourseListViewModel>());
            }
        }

        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> Create()
        {
            var viewModel = new CreateCourseViewModel();
            await PopulateCreateCourseListsAsync(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCourseViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCreateCourseListsAsync(viewModel);
                return View(viewModel);
            }

            var course = _mapper.Map<Course>(viewModel);
            course.InstructorId = GetCurrentUserId();
            course.Status = CourseStatus.Pending;
            course.TotalDuration = 0;

            if (viewModel.SelectedTagIds != null && viewModel.SelectedTagIds.Any())
            {
                course.CourseTags = viewModel.SelectedTagIds
                    .Distinct()
                    .Select(tagId => new CourseTag { TagId = tagId })
                    .ToList();
            }

            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(viewModel.ImageFile.FileName)}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using var fileStream = new FileStream(filePath, FileMode.Create);
                await viewModel.ImageFile.CopyToAsync(fileStream);
                course.ImageUrl = $"/images/courses/{uniqueFileName}";
            }
            else
            {
                course.ImageUrl = "/images/default-course.jpg";
            }

            var createdCourse = await _courseService.CreateCourseAsync(course);
            TempData["SuccessMessage"] = "Course basics saved. Build your sections and lessons now, and the total duration will update automatically.";
            return RedirectToAction("ManageContent", "Lessons", new { courseId = createdCourse.Id });
        }

        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> Edit(int id)
        {
            var result = await _courseService.GetCourseForEditAsync(id, GetCurrentUserId(), User.IsInRole("Admin"));
            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.IsForbidden)
            {
                return Forbid();
            }

            return View(result.ViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, EditCourseViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _courseService.UpdateCourseAsync(model, GetCurrentUserId(), User.IsInRole("Admin"));
            if (result.NotFound)
            {
                return NotFound();
            }

            if (result.IsForbidden)
            {
                return Forbid();
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.Errors.FirstOrDefault() ?? "We could not save your changes right now.");
                return View(model);
            }

            TempData["SuccessMessage"] = result.Message ?? "Course information updated successfully.";
            return RedirectToAction("MyCourses", "Instructor");
        }

        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> Delete(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            if (course.InstructorId != GetCurrentUserId())
                return Forbid();

            return View(course);
        }

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null)
                return NotFound();

            if (course.InstructorId != GetCurrentUserId())
                return Forbid();

            try
            {
                var success = await _courseService.DeleteCourseAsync(id);
                if (!success)
                    return NotFound();

                TempData["Success"] = "Course deleted successfully.";
            }
            catch
            {
                TempData["Error"] = "We could not delete the course right now. Please try again.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var course = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Currency)
                .Include(c => c.Instructor)
                .Include(c => c.Reviews)
                    .ThenInclude(r => r.Student)
                .Include(c => c.CourseTags)
                    .ThenInclude(ct => ct.Tag)
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (course == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canPreviewUnapproved = User.IsInRole("Admin") || (!string.IsNullOrEmpty(userId) && course.InstructorId.ToString() == userId);
            if (course.Status != CourseStatus.Approved && !canPreviewUnapproved)
            {
                return NotFound();
            }

            var isEnrolled = false;
            var isOwner = !string.IsNullOrEmpty(userId) && course.InstructorId.ToString() == userId;
            var progressPercentage = 0;
            var completedLessons = 0;
            var hasReviewed = false;
            Review? existingReview = null;
            var firstLessonId = course.Sections
                .OrderBy(section => section.OrderIndex)
                .SelectMany(section => section.Lessons.OrderBy(lesson => lesson.OrderIndex))
                .Select(lesson => (int?)lesson.Id)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(userId))
            {
                var enrollment = await _context.Enrollments.FirstOrDefaultAsync(e => e.StudentId.ToString() == userId && e.CourseId == id);
                isEnrolled = enrollment != null;
                existingReview = course.Reviews.FirstOrDefault(review => review.StudentId.ToString() == userId);
                hasReviewed = existingReview != null;

                if (enrollment != null)
                {
                    var progressSummary = await _learningService.GetCourseProgressAsync(course.Id, enrollment.StudentId);
                    progressPercentage = progressSummary.ProgressPercentage;
                    completedLessons = progressSummary.CompletedLessons;
                    firstLessonId = progressSummary.ContinueLessonId ?? firstLessonId;
                }
            }

            var viewModel = new CourseDetailsViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                Price = course.Price,
                CurrencyName = course.Currency?.Name ?? "Unknown",
                CurrencySymbol = course.Currency?.Symbol ?? "$",
                ImageUrl = course.ImageUrl,
                CategoryName = course.Category?.Title ?? "General",
                InstructorId = course.InstructorId,
                InstructorName = $"{course.Instructor?.FirstName} {course.Instructor?.LastName}".Trim(),
                InstructorEmail = course.Instructor?.Email ?? string.Empty,
                InstructorProfilePicture = course.Instructor?.ProfilePicture ?? "/images/profiles/default-avatar.png",
                Level = course.Level.ToString(),
                Status = course.Status.ToString(),
                Language = course.Language,
                AverageRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = course.Reviews.Count(),
                SectionCount = course.Sections.Count,
                TotalLessons = course.Sections.Sum(s => s.Lessons.Count),
                TotalDuration = course.Sections.Sum(s => s.Lessons.Sum(l => l.Duration)),
                IsEnrolled = isEnrolled,
                IsOwner = isOwner,
                ProgressPercentage = progressPercentage,
                CompletedLessons = completedLessons,
                CanReview = isEnrolled && !isOwner,
                HasReviewed = hasReviewed,
                ReviewForm = new AddReviewViewModel
                {
                    Rating = existingReview?.Rating ?? 5,
                    Comment = existingReview?.Comment ?? string.Empty
                },
                FirstLessonId = firstLessonId,
                Tags = course.CourseTags
                    .Where(ct => ct.Tag != null)
                    .Select(ct => ct.Tag!.Name)
                    .ToList(),
                Sections = course.Sections.Select(s => new SectionDetailsViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    Lessons = s.Lessons.Select(l => new LessonDetailsViewModel
                    {
                        Id = l.Id,
                        Title = l.Title,
                        Duration = l.Duration,
                        IsFree = l.IsFree,
                        Type = l.Type
                    }).ToList()
                }).ToList(),
                Reviews = course.Reviews
                    .OrderByDescending(review => review.CreatedAt)
                    .Select(review => new ReviewDetailsViewModel
                    {
                        Rating = review.Rating,
                        Comment = review.Comment,
                        CreatedAt = review.CreatedAt,
                        StudentName = $"{review.Student?.FirstName} {review.Student?.LastName}".Trim()
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int id, AddReviewViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values
                    .SelectMany(entry => entry.Errors)
                    .Select(error => error.ErrorMessage)
                    .FirstOrDefault() ?? "Please review your rating and comment, then try again.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            var studentId = int.Parse(userId);
            var course = await _context.Courses
                .Include(c => c.Reviews)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (course.InstructorId == studentId)
            {
                TempData["Error"] = "Course owners cannot review their own courses.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var isEnrolled = await _context.Enrollments.AnyAsync(e => e.CourseId == id && e.StudentId == studentId);
            if (!isEnrolled)
            {
                TempData["Error"] = "You can review a course only after enrolling in it.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var existingReview = course.Reviews.FirstOrDefault(review => review.StudentId == studentId);
            if (existingReview == null)
            {
                _context.Reviews.Add(new Review
                {
                    CourseId = id,
                    StudentId = studentId,
                    Rating = model.Rating,
                    Comment = model.Comment.Trim(),
                    CreatedAt = DateTime.UtcNow
                });
                TempData["Success"] = "Your course review was added successfully.";
            }
            else
            {
                existingReview.Rating = model.Rating;
                existingReview.Comment = model.Comment.Trim();
                existingReview.CreatedAt = DateTime.UtcNow;
                TempData["Success"] = "Your course review was updated successfully.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task PopulateCreateCourseListsAsync(CreateCourseViewModel viewModel)
        {
            var categories = await _categoryRepo.GetAllAsync() ?? Enumerable.Empty<Category>();
            var currencies = await _currencyRepo.GetAllAsync() ?? Enumerable.Empty<Currency>();
            var tags = await _tagRepo.GetAllAsync() ?? Enumerable.Empty<Tag>();

            viewModel.CategoriesList = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title
            });

            viewModel.CurrenciesList = currencies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.Code} - {c.Name} ({c.Symbol})"
            });

            viewModel.TagsList = tags.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            });
        }

        private int GetCurrentUserId()
        {
            var idClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return idClaim?.Value != null ? int.Parse(idClaim.Value) : 0;
        }
    }
}
