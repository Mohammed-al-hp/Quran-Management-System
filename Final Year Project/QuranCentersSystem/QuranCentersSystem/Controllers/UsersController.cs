using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// متحكم المستخدمين - إدارة حسابات النظام (للمدير فقط)
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.ApplicationUsers.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string Password)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(Password))
            {
                // أ- إنشاء المستخدم في نظام Identity لتشفير كلمة المرور
                var identityUser = new IdentityUser { UserName = user.Email, Email = user.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(identityUser, Password);

                if (result.Succeeded)
                {
                    // ب- إسناد الدور المختار (مدير النظام أو محفظ)
                    await _userManager.AddToRoleAsync(identityUser, user.Role);

                    // ج- حفظ في جدول العرض المخصص
                    user.IsActive = true;
                    _context.ApplicationUsers.Add(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("Index", await _context.ApplicationUsers.ToListAsync());
        }
    }
}