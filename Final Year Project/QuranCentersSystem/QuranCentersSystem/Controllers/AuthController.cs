using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QuranCentersSystem.Models;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (model == null) return BadRequest();

        // 1. البحث عن المستخدم بواسطة البريد الإلكتروني (الموجود في قاعدة بياناتك)
        var user = await _userManager.FindByEmailAsync(model.Username);

        if (user == null)
        {
            // محاولة البحث باسم المستخدم إذا لم ينجح بالبريد
            user = await _userManager.FindByNameAsync(model.Username);
        }

        if (user != null)
        {
            // 2. التحقق من كلمة المرور
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                // 3. جلب دور المستخدم (Admin, Teacher, إلخ)
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    success = true,
                    message = "تم تسجيل الدخول بنجاح",
                    userId = user.Id,
                    email = user.Email,
                    role = roles.FirstOrDefault() // نرسل الدور للتطبيق لتحديد الصلاحيات في Flutter
                });
            }
        }

        return Unauthorized(new { success = false, message = "بيانات الدخول غير صحيحة" });
    }
}