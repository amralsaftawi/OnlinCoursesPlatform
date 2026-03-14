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
        public async Task<ActionResult> Index(int page = 1)
        {
            try
            {
                int pageSize = 10; // عدد الكورسات في الصفحة الواحدة

                // بنكلم الـ Service وناخد منها الداتا والعدد
                var result = await _courseService.GetPaginatedCoursesAsync(page, pageSize);

                // حساب عدد الصفحات الكلي
                var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);

                // بنبعت الأرقام دي للـ View عشان يرسم زراير (Next & Previous)
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                //  "It's preferred to convert result.Courses to CourseListViewModel"
                return View(result.Courses);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "An error occurred while retrieving courses: " + ex.Message;
                return View(new List<Course>());
            }
        }

        // GET: CoursesController/Details/5
        public async Task<ActionResult> Details(int id)
        {
            var viewModel = await _courseService.GetCourseDetailsProjectedAsync(id);
            if (viewModel == null)
                return NotFound();

            return View(viewModel);
        }

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
    }
}
