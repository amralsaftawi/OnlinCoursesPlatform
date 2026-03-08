using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlinCoursePlatform.ViewModels;
using OnlineCoursesPlatform.Models;
using OnlineCoursesPlatform.Models.Enums;

public class AccountController(UserManager<User> userManager, SignInManager<User> signInManager) : Controller
{
    public IActionResult Register() => View();

 

[HttpPost]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    // 1. فحص الإيميل
    var existingUser = await userManager.FindByEmailAsync(model.Email);
    if (existingUser != null)
    {
        ModelState.AddModelError("Email", "This email is already registered.");
        return View(model);
    }

    // 2. إنشاء اليوزر
    var user = new User 
    { 
        UserName = model.Email, 
        Email = model.Email, 
        FirstName = model.FirstName, 
        LastName = model.LastName,
        Role = model.IsInstructor ? UserRole.Instructor : UserRole.Student 
    };

    // 3. الحفظ في الداتابيز
    var result = await userManager.CreateAsync(user, model.Password);

    if (result.Succeeded)
    {
        // 4. الحل الأكيد: بنادي على AddToRoleAsync باسم الرول 
        // الـ user object هنا خلاص بقى فيه الـ ID بعد الـ CreateAsync الناجحة
        var roleResult = await userManager.AddToRoleAsync(user, user.Role.ToString());

        if (roleResult.Succeeded)
        {
            await signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Home");
        }
    }

    foreach (var error in result.Errors) 
        ModelState.AddModelError("", error.Description);

    return View(model);
}

    public IActionResult Login() => View();

 [HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    if (ModelState.IsValid)
    {
        // 1. بندور على اليوزر بالإيميل الأول
        var user = await userManager.FindByEmailAsync(model.Email);
        
        if (user != null)
        {
            // 2. بنعمل تسجيل دخول باستخدام كائن اليوزر نفسه
            var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);
            
            if (result.Succeeded) 
                return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "The email or password wrong.");
    }
    return View(model);
}
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [Authorize] // لازم يكون مسجل دخول عشان يشوف البروفايل
public async Task<IActionResult> Profile()
{
    var user = await userManager.GetUserAsync(User);
    if (user == null) return NotFound();

    return View(user);
}


[Authorize]
public async Task<IActionResult> EditProfile()
{
    var user = await userManager.GetUserAsync(User);
    if (user == null) return NotFound();

    var model = new EditProfileViewModel
    {
        FirstName = user.FirstName,
        LastName = user.LastName,
        // السطر ده هو اللي هيخلي الصورة تظهر لما تفتح الصفحة
        ExistingImageUrl = user.ProfilePicture 
    };

    return View(model);
}
[HttpPost]
[Authorize]
[ValidateAntiForgeryToken] // حماية من هجمات الـ CSRF
public async Task<IActionResult> EditProfile(EditProfileViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    // 1. جلب بيانات اليوزر الحالي من الداتابيز
    var user = await userManager.GetUserAsync(User);
    if (user == null) return NotFound();

    // 2. تحديث البيانات النصية
    user.FirstName = model.FirstName;
    user.LastName = model.LastName;

    // 3. التعامل مع رفع الصورة الجديدة
    if (model.ProfileImage != null)
    {
        // تحديد مسار فولدر الصور والتأكد من وجوده
        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "profiles");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // مسح الصورة القديمة من السيرفر (لو مش هي الصورة الافتراضية)
        if (!string.IsNullOrEmpty(user.ProfilePicture) && user.ProfilePicture != "default-avatar.png")
        {
            string oldPath = Path.Combine(uploadsFolder, user.ProfilePicture);
            if (System.IO.File.Exists(oldPath))
            {
                System.IO.File.Delete(oldPath);
            }
        }

        // توليد اسم فريد وحفظ الصورة الجديدة
        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.ProfileImage.FileName);
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await model.ProfileImage.CopyToAsync(fileStream);
        }

        // تحديث اسم الملف في موديل اليوزر
        user.ProfilePicture = uniqueFileName;
    }

    // 4. حفظ التغييرات في الداتابيز
    var result = await userManager.UpdateAsync(user);

    if (result.Succeeded)
    {
        // أهم سطر: بيحدث الـ Cookie عشان الصورة والاسم الجديد يظهروا في الـ Navbar فوراً
        await signInManager.RefreshSignInAsync(user);
        
        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction("Profile");
    }

    // لو فيه أخطاء من الـ Identity (زي إن الـ UserName متكرر مثلاً)
    foreach (var error in result.Errors)
    {
        ModelState.AddModelError("", error.Description);
    }

    return View(model);
}
}