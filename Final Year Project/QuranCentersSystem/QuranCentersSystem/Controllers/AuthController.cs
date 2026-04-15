using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuranCentersSystem.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
// احذف سطر [Authorize] من هنا لكي يتمكن المستخدم من طلب الـ login
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(UserManager<ApplicationUser> userManager,
                          SignInManager<ApplicationUser> signInManager,
                          IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }
    [AllowAnonymous] // أضف هذا السطر هنا للتأكد
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        if (model == null || string.IsNullOrEmpty(model.Username))
            return BadRequest(new { message = "بيانات الدخول مطلوبة" });

        // 1. البحث عن المستخدم (بالإيميل أو اسم المستخدم) كما هو في قاعدة QuranCentersDB
        var user = await _userManager.FindByEmailAsync(model.Username)
                   ?? await _userManager.FindByNameAsync(model.Username);

        if (user != null)
        {
            // 2. التحقق من كلمة المرور
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                // 3. التحقق من حالة الحساب النشطة (IsActive) التي أضفناها
                if (!user.IsActive)
                {
                    return Unauthorized(new { success = false, message = "هذا الحساب معطل حالياً" });
                }

                // 4. جلب الأدوار (Roles)
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? user.Role ?? "NoRole";

                // 5. توليد الـ JWT Token ليعمل مع Flutter
                var token = GenerateJwtToken(user, userRole);

                return Ok(new
                {
                    success = true,
                    message = "تم تسجيل الدخول بنجاح",
                    token = token, // هذا النص هو ما سيحفظه Flutter
                    userId = user.Id,
                    email = user.Email,
                    role = userRole
                });
            }
        }

        return Unauthorized(new { success = false, message = "اسم المستخدم أو كلمة المرور غير صحيحة" });
    }

    private string GenerateJwtToken(ApplicationUser user, string role)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Role, role)
        };

        // ملاحظة: يجب أن يكون لديك Key في ملف appsettings.json
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "YourSuperSecretKey12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(7); // صلاحية الدخول أسبوع

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Audience"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}