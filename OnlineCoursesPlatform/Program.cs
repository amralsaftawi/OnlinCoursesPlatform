using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlinCoursePlatform.Abstrctions;
using OnlinCoursePlatform.Services;
using OnlinCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;
using OnlineCoursesPlatform.Repositories;
using OnlineCoursesPlatform.Repositories.Interface;
using OnlineCoursesPlatform.Services;
using OnlineCoursesPlatform.Services.Interfaces;
using System.Reflection;
//using OnlineCoursesPlatform.Mappings;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Repositories & Services
builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<ILessonService, LessonService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews();

var app = builder.Build();


// ✅ SEEDING (مرة واحدة بس)
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    string[] roles = { "Admin", "Instructor", "Student" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }

    // 🎯 Create Admin لو مش موجود
    var admin = await userManager.FindByNameAsync("admin");

    if (admin == null)
    {
        admin = new User
        {
            UserName = "admin",
            Email = "admin@test.com",
            FirstName = "System",
            LastName = "Admin"
        };

        var result = await userManager.CreateAsync(admin, "Admin123!");

        if (!result.Succeeded)
            throw new Exception(string.Join(",", result.Errors.Select(e => e.Description)));

        // ✅ Assign Role
        await userManager.AddToRoleAsync(admin, "Admin");

        // ✅ Create Admin Profile
        context.AdminProfiles.Add(new AdminProfile
        {
            ApplicationUserId = admin.Id
        });

        await context.SaveChangesAsync();
    }
}


// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();




// Seeding Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

    // هنجيب كل الأسماء اللي جوه الـ Enum بتاعك
    var roles = Enum.GetNames(typeof(UserRole));

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
        }
    }
}
app.Run();
