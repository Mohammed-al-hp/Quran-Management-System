using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// متحكم المصادقة - يدير عمليات تسجيل الدخول والتسجيل مع دعم JWT للتطبيق المحمول
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// تسجيل الدخول - يدعم كلاً من البريد الإلكتروني واسم المستخدم
        /// يُرجع رمز JWT عند نجاح المصادقة
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { success = false, message = "جميع الحقول مطلوبة" });

            // البحث عن المستخدم بواسطة البريد الإلكتروني أو اسم المستخدم
            var user = await _userManager.FindByEmailAsync(model.Username)
                       ?? await _userManager.FindByNameAsync(model.Username);

            if (user == null)
                return Unauthorized(new { success = false, message = "بيانات الدخول غير صحيحة" });

            // التحقق من حالة الحساب النشطة
            if (!user.IsActive)
                return Unauthorized(new { success = false, message = "هذا الحساب معطل حالياً" });

            // التحقق من كلمة المرور
            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault() ?? user.Role ?? "Student";
                var token = GenerateJwtToken(user, role);

                return Ok(new
                {
                    success = true,
                    message = "تم تسجيل الدخول بنجاح",
                    token = token,
                    userId = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    role = role
                });
            }

            return Unauthorized(new { success = false, message = "بيانات الدخول غير صحيحة" });
        }

        /// <summary>
        /// تسجيل مستخدم جديد مع تعيين الدور المناسب
        /// </summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "بيانات غير صالحة" });

            var validRoles = new[] { "Admin", "Teacher", "Student", "Parent" };
            if (!validRoles.Contains(model.Role))
                return BadRequest(new { success = false, message = "الدور المحدد غير صالح" });

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Conflict(new { success = false, message = "البريد الإلكتروني مسجل مسبقاً" });

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true,
                Role = model.Role,
                IsActive = true,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                var token = GenerateJwtToken(user, model.Role);

                return Ok(new
                {
                    success = true,
                    message = "تم إنشاء الحساب بنجاح",
                    userId = user.Id,
                    email = user.Email,
                    fullName = user.FullName,
                    role = model.Role,
                    token = token
                });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { success = false, message = errors });
        }

        /// <summary>
        /// الحصول على معلومات المستخدم الحالي
        /// </summary>
        [Authorize(AuthenticationSchemes = "Bearer")]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return NotFound(new { success = false, message = "المستخدم غير موجود" });

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                success = true,
                userId = user.Id,
                email = user.Email,
                fullName = user.FullName,
                role = roles.FirstOrDefault() ?? user.Role,
                isActive = user.IsActive
            });
        }

        /// <summary>
        /// توليد رمز JWT مع المعلومات المطلوبة
        /// </summary>
        private string GenerateJwtToken(ApplicationUser user, string role)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "ItqanQuranSystem2026SecureKeyMustBe32CharsOrMore!!";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("fullName", user.FullName ?? "")
            };

            var expiryMinutes = int.Parse(jwtSettings["ExpiryInMinutes"] ?? "1440");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "ItqanQuranSystem",
                audience: jwtSettings["Audience"] ?? "ItqanFlutterApp",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    /// <summary>
    /// نموذج التسجيل
    /// </summary>
    public class RegisterModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [System.ComponentModel.DataAnnotations.EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [System.ComponentModel.DataAnnotations.MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 أحرف على الأقل")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "الدور مطلوب")]
        public string Role { get; set; }

        public string? FullName { get; set; }
    }
}