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
            // جلب قائمة المستخدمين من قاعدة البيانات
            var users = await _context.Users.ToListAsync();
            return View(users);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ApplicationUser user, string Password, string Role)
        {
            if (ModelState.IsValid && !string.IsNullOrEmpty(Password))
            {
                // 1. إنشاء المستخدم في Identity
                var newUser = new ApplicationUser
                {
                    UserName = user.Email,
                    Email = user.Email,
                    EmailConfirmed = true,
                   // Name = user.Name, // تأكد أن لديك حقل Name في ApplicationUser
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(newUser, Password);

                if (result.Succeeded)
                {
                    // 2. إسناد الدور (مدير، محفظ، إلخ)
                    if (!string.IsNullOrEmpty(Role))
                    {
                        await _userManager.AddToRoleAsync(newUser, Role);
                    }

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