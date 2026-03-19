using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.ViewModels;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services.Interfaces;
using OnlineCoursesPlatform.ViewModels;
using System.Security.Claims;

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

        public CoursesController(ICourseService courseService, AppDbContext context, IMapper mapper,
        IRepository<Category> categoryRepo,
        IRepository<Currency> currencyRepo,
        IRepository<Tag> tagRepo,
        IWebHostEnvironment webHostEnvironment)
        {
            _courseService = courseService;
            _context = context;
            _mapper = mapper;
            _categoryRepo = categoryRepo;
            _currencyRepo = currencyRepo;
            _tagRepo = tagRepo;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: CoursesController

[HttpGet]
public async Task<ActionResult> Index(string searchTerm, int? categoryId, List<int> selectedTags, int page = 1)
{
    // Initialize default values to prevent NullReference in View
    ViewBag.Categories = new SelectList(Enumerable.Empty<Category>(), "Id", "Title");
    ViewBag.Tags = Enumerable.Empty<Tag>();
    ViewBag.SelectedTags = selectedTags ?? new List<int>();
    ViewBag.CurrentSearch = searchTerm;

    try
    {
        const int PageSize = 6;

        // 1. Fetch Data
        var allCourses = await _courseService.GetAllCoursesAsync() ?? Enumerable.Empty<Course>();
        var coursesQuery = allCourses.AsQueryable();

        // 2. Load Filter Data (Dropdowns & Checkboxes)
        var categories = await _categoryRepo.GetAllAsync() ?? Enumerable.Empty<Category>();
        var tags = await _tagRepo.GetAllAsync() ?? Enumerable.Empty<Tag>();

        ViewBag.Categories = new SelectList(categories, "Id", "Title", categoryId);
        ViewBag.Tags = tags;

        // 3. Filtering Logic
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            coursesQuery = coursesQuery.Where(c => 
                c.Title.ToLower().Contains(term) || 
                (c.Description != null && c.Description.ToLower().Contains(term))
            );
        }

        if (categoryId.HasValue && categoryId > 0)
        {
            coursesQuery = coursesQuery.Where(c => c.CategoryId == categoryId.Value);
        }

        if (selectedTags != null && selectedTags.Any())
        {
            coursesQuery = coursesQuery.Where(c => 
                c.CourseTags != null && c.CourseTags.Any(ct => selectedTags.Contains(ct.TagId))
            );
        }

        // 4. Pagination
        int totalCount = coursesQuery.Count();
        int totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

        var paginatedCourses = coursesQuery
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToList();

        // 5. Mapping
        var mappedCourses = _mapper.Map<IEnumerable<CourseListViewModel>>(paginatedCourses);

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = totalPages;
        ViewBag.SelectedCategory = categoryId;

        return View(mappedCourses);
    }
    catch (Exception ex)
    {
        // Log your error here
        ViewBag.Error = "We encountered an issue loading the filters, but you can still browse courses.";
        return View(new List<CourseListViewModel>());
    }
}




       // GET: CoursesController
     // GET: CoursesController/Create
        [HttpGet]
        [Authorize(Roles = "Instructor")]
        public async Task<ActionResult> Create()
        {
            CreateCourseViewModel viewModel = new CreateCourseViewModel();

            // Prepare dropdown data
            var categories = await _categoryRepo.GetAllAsync();
            var currencies = await _currencyRepo.GetAllAsync();
            var tags = await _tagRepo.GetAllAsync();
            // Map to SelectListItem for dropdowns
            viewModel.CategoriesList = categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Title
            });

            viewModel.CurrenciesList = currencies.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            });

            viewModel.TagsList = tags.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = t.Name
            });

            return View(viewModel);
        }


        // POST: CoursesController/Create
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateCourseViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryRepo.GetAllAsync();
                var currencies = await _currencyRepo.GetAllAsync();
                var tags = await _tagRepo.GetAllAsync();

                viewModel.CategoriesList = categories.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Title
                });

                viewModel.CurrenciesList = currencies.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });

                viewModel.TagsList = tags.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = t.Name
                });

                return View(viewModel);
            }

            // Map ViewModel to Course entity using AutoMapper ⭐
            var course = _mapper.Map<Course>(viewModel);
            course.InstructorId = GetLoginInstructor();

            // Map standard SelectedTagIds to CourseTags
            if (viewModel.SelectedTagIds != null && viewModel.SelectedTagIds.Any())
            {
                course.CourseTags = viewModel.SelectedTagIds.Select(tagId => new CourseTag
                {
                    TagId = tagId
                }).ToList();
            }

            // Handle Image Upload
            if (viewModel.ImageFile != null && viewModel.ImageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "courses");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + viewModel.ImageFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await viewModel.ImageFile.CopyToAsync(fileStream);
                }

                course.ImageUrl = "/images/courses/" + uniqueFileName;
            }
            else
            {
                // Default image if none uploaded
                course.ImageUrl = "/images/default-course.jpg";
            }

            await _courseService.CreateCourseAsync(course);
            TempData["Success"] = "Course created successfully! 🎉";
            return RedirectToAction(nameof(Index));
        }

        // GET: CoursesController/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound();

                return View(course);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View();
            }
        }

        // POST: CoursesController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, Course course)
        {
            try
            {
                if (id != course.Id)
                    return BadRequest();

                if (!ModelState.IsValid)
                    return View(course);

                await _courseService.UpdateCourseAsync(course);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while updating the course: " + ex.Message;
                return View(course);
            }
        }

        // GET: CoursesController/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                if (course == null)
                    return NotFound();

                return View(course);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View();
            }
        }

        // POST: CoursesController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, IFormCollection collection)
        {
            try
            {
                var success = await _courseService.DeleteCourseAsync(id);
                if (!success)
                    return NotFound();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while deleting the course: " + ex.Message;
                return View();
            }
        }

        // Helper method to get current authenticated user ID
        private int GetLoginInstructor()
        {
            Claim IDclaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return IDclaim.Value != null ? int.Parse(IDclaim.Value) : 0;
        }


        
       [HttpGet]
public async Task<IActionResult> Details(int? id)
{
    if (id == null) return NotFound();

    // 1. جلب الكورس مع كل العلاقات اللازمة
    var course = await _context.Courses
        .Include(c => c.Category)
        .Include(c => c.Instructor)
        .Include(c => c.Reviews)
        .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
            .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
        .FirstOrDefaultAsync(m => m.Id == id);

    if (course == null) return NotFound();

    // 2. التحقق من حالة الاشتراك لليوزر الحالي
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    bool isEnrolled = false;

    if (!string.IsNullOrEmpty(userId))
    {
        // افترضنا إن عندك جدول اسمه Enrollments بيربط اليوزر بالكورس
        isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId.ToString() == userId && e.CourseId == id);
    }

    // 3. بناء الـ ViewModel
    var viewModel = new CourseDetailsViewModel
    {
        Id = course.Id,
        Title = course.Title,
        Description = course.Description,
        Price = course.Price,
        CurrencySymbol = "$", 
        ImageUrl = course.ImageUrl,
        CategoryName = course.Category?.Title ?? "General",
        InstructorName = $"{course.Instructor?.FirstName} {course.Instructor?.LastName}",
        InstructorProfilePicture = course.Instructor?.ProfilePicture,
        Level = course.Level.ToString(),
        AverageRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0,
        ReviewCount = course.Reviews.Count(),
        SectionCount = course.Sections.Count,
        TotalLessons = course.Sections.Sum(s => s.Lessons.Count),
        TotalDuration = course.Sections.Sum(s => s.Lessons.Sum(l => l.Duration)),
        
        // القيمة الحقيقية للاشتراك
        IsEnrolled = isEnrolled, 
        
        Sections = course.Sections.Select(s => new SectionDetailsViewModel {
            Id = s.Id,
            Title = s.Title,
            Lessons = s.Lessons.Select(l => new LessonDetailsViewModel {
                Id = l.Id,
                Title = l.Title,
                Duration = l.Duration,
                // تأكد إن خاصية IsFree موجودة في الموديل بتاعك وViewModel
                IsFree = l.IsFree, 
                Type = l.Type
            }).ToList()
        }).ToList()
    };

    return View(viewModel);
}
    }
}
