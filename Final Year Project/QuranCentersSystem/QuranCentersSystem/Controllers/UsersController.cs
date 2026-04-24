using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string Password)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(Password))
            {
                // أ- إنشاء المستخدم في نظام Identity لتشفير كلمة المرور
                var ApplicationUser = new ApplicationUser { UserName = user.Email, Email = user.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(ApplicationUser, Password);

                if (result.Succeeded)
                {
                    // ب- إسناد الدور المختار (مدير النظام أو محفظ)
                    await _userManager.AddToRoleAsync(ApplicationUser, user.Role);

                    // ج- حفظ في جدول العرض المخصص
                    user.IsActive = true;
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View("Index", await _context.Users.ToListAsync());
        }
    }
}