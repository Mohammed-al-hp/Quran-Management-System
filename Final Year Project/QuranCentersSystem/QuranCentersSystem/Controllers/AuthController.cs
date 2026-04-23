<<<<<<< HEAD
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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }
=======
﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
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
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad

        /// <summary>
        /// تسجيل الدخول - يدعم كلاً من البريد الإلكتروني واسم المستخدم
        /// يُرجع رمز JWT عند نجاح المصادقة
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { success = false, message = "جميع الحقول مطلوبة" });

            // 1. البحث عن المستخدم بواسطة البريد الإلكتروني
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
                    // 3. جلب دور المستخدم
                    var roles = await _userManager.GetRolesAsync(user);
                    var role = roles.FirstOrDefault() ?? "Student";

                    // 4. إنشاء رمز JWT
                    var token = GenerateJwtToken(user, role);

                    return Ok(new
                    {
                        success = true,
                        message = "تم تسجيل الدخول بنجاح",
                        userId = user.Id,
                        email = user.Email,
                        role = role,
                        token = token
                    });
                }
            }

            return Unauthorized(new { success = false, message = "بيانات الدخول غير صحيحة" });
        }

        /// <summary>
        /// تسجيل مستخدم جديد مع تعيين الدور المناسب
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model == null || !ModelState.IsValid)
                return BadRequest(new { success = false, message = "بيانات غير صالحة" });

            // التحقق من أن الدور المطلوب صالح
            var validRoles = new[] { "Admin", "Teacher", "Student", "Parent" };
            if (!validRoles.Contains(model.Role))
                return BadRequest(new { success = false, message = "الدور المحدد غير صالح" });

            // التحقق من عدم وجود المستخدم مسبقاً
            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
                return Conflict(new { success = false, message = "البريد الإلكتروني مسجل مسبقاً" });

            var user = new IdentityUser
            {
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
<<<<<<< HEAD
                // تعيين الدور (Discriminator) بشكل صحيح
                await _userManager.AddToRoleAsync(user, model.Role);

                var token = GenerateJwtToken(user, model.Role);
=======
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
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad

                return Ok(new
                {
                    success = true,
<<<<<<< HEAD
                    message = "تم إنشاء الحساب بنجاح",
                    userId = user.Id,
                    email = user.Email,
                    role = model.Role,
                    token = token
=======
                    message = "تم تسجيل الدخول بنجاح",
                    token = token, // هذا النص هو ما سيحفظه Flutter
                    userId = user.Id,
                    email = user.Email,
                    role = userRole
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
                });
            }

            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new { success = false, message = errors });
        }

<<<<<<< HEAD
        /// <summary>
        /// توليد رمز JWT مع المعلومات المطلوبة
        /// </summary>
        private string GenerateJwtToken(IdentityUser user, string role)
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
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

        public string FullName { get; set; }
=======
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
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
    }
}