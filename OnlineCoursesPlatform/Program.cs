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

// Add services to the container.

// استبدل DefaultConnection بالاسم الموجود في appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();
//for generic repository 
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ILearningService, LearningService>();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
//  AutoMapper Configuration
//builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IInstructorService, InstructorService>();
builder.Services.AddScoped<ILessonService, LessonService>();

// إضافة خدمات الـ Identity وربطها بالـ DbContext والـ Role
builder.Services.AddIdentity<User, IdentityRole<int>>(options => {
    // إعدادات اختيارية (مثلاً المتطلبات الخاصة بكلمة السر)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<AppDbContext>() // ده السطر اللي كان ناقص عشان يربط الـ UserManager بالداتابيز
.AddDefaultTokenProviders();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
