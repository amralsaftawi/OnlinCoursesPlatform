using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineCoursesPlatform.Data;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Repositories;
using OnlineCoursesPlatform.Repositories.Interfaces;
using OnlineCoursesPlatform.Services;
using OnlineCoursesPlatform.Services.Interfaces;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<ILearningService, LearningService>();
builder.Services.AddScoped<ILessonService, LessonService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

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

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    string[] roles = ["Admin", "Instructor", "Student"];

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }

    var admin = await userManager.FindByEmailAsync("admin@test.com") ?? await userManager.FindByNameAsync("admin");

    if (admin == null)
    {
        admin = new User
        {
            UserName = "admin",
            Email = "admin@test.com",
            FirstName = "System",
            LastName = "Admin"
        };

        var createResult = await userManager.CreateAsync(admin, "Admin123!");
        if (!createResult.Succeeded)
        {
            throw new Exception(string.Join(", ", createResult.Errors.Select(error => error.Description)));
        }

        await userManager.AddToRoleAsync(admin, "Admin");

        context.AdminProfiles.Add(new AdminProfile
        {
            ApplicationUserId = admin.Id
        });

        await context.SaveChangesAsync();
    }

    if (!await userManager.IsInRoleAsync(admin, "Admin"))
    {
        await userManager.AddToRoleAsync(admin, "Admin");
    }

    if (!await userManager.IsInRoleAsync(admin, "Student"))
    {
        await userManager.AddToRoleAsync(admin, "Student");
    }

    var allUsers = await userManager.Users.ToListAsync();
    foreach (var platformUser in allUsers)
    {
        var existingRoles = await userManager.GetRolesAsync(platformUser);
        if (!existingRoles.Contains("Student"))
        {
            await userManager.AddToRoleAsync(platformUser, "Student");
        }
    }

    if (!context.Categories.Any())
    {
        context.Categories.AddRange(
            new Category { Title = "Development" },
            new Category { Title = "Business" },
            new Category { Title = "Design" },
            new Category { Title = "Marketing" });
    }

    if (!context.Currencies.Any())
    {
        context.Currencies.AddRange(
            new Currency { Name = "Egyptian Pound", Code = "EGP", Symbol = "E£" },
            new Currency { Name = "US Dollar", Code = "USD", Symbol = "$" },
            new Currency { Name = "Euro", Code = "EUR", Symbol = "EUR" });
    }

    if (!context.Tags.Any())
    {
        context.Tags.AddRange(
            new Tag { Name = "backend" },
            new Tag { Name = "frontend" },
            new Tag { Name = "beginner" },
            new Tag { Name = "advanced" },
            new Tag { Name = "api" },
            new Tag { Name = "database" });
    }

    await context.SaveChangesAsync();
}

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

app.Run();
