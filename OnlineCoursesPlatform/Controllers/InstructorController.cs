using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Dtos;
using OnlineCoursesPlatform.Models;

namespace OnlinCoursesPlatform.Controllers;

public class InstructorController(UserManager<User> userManager,IInstructorService instructorService) : Controller
{
    private readonly IInstructorService _instructorService=instructorService;
    private readonly UserManager<User> _userManager=userManager;


public async Task<IActionResult> Index()
{
    var stats = new InstructorStatsDto
    {
        TotalCourses = 3,
        TotalStudents = 120,
        TotalRevenue = 4500
    };

    return View(stats);
}

  public async Task<IActionResult> MyCourses()
{
    // داتا وهمية للتيست فقط
    var fakeCourses = new List<InstructorCourseDto>
    {
        new InstructorCourseDto { 
            Id = 1, Title = "Mastering ASP.NET Core 9", 
            CategoryName = "Development", Price = 49.99m, 
            EnrollmentsCount = 150, Status = "Published",
            ImageUrl = "https://placehold.co/600x400/5e72e4/white?text=ASP.NET+Core"
        },
        new InstructorCourseDto { 
            Id = 2, Title = "Advanced SQL Server", 
            CategoryName = "Database", Price = 35.00m, 
            EnrollmentsCount = 85, Status = "Published",
            ImageUrl = "https://placehold.co/600x400/2dce89/white?text=SQL+Server"
        },
        new InstructorCourseDto { 
            Id = 3, Title = "Entity Framework Pro", 
            CategoryName = "Development", Price = 29.00m, 
            EnrollmentsCount = 0, Status = "Draft",
            ImageUrl = "https://placehold.co/600x400/11cdef/white?text=EF+Core"
        }
    };

    return View(fakeCourses);
}
}