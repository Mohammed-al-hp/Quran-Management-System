using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using QuranCenters.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// متحكم المستخدمين - إدارة حسابات النظام (للمدير فقط)
    /// يستخدم UserManager بدلاً من DbContext مباشرة لإدارة المستخدمين
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string Email, string FullName, string Role, string Password)
        {
            if (!string.IsNullOrEmpty(Email) && !string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(Role))
            {
                var user = new ApplicationUser
                {
                    UserName = Email,
                    Email = Email,
                    EmailConfirmed = true,
                    FullName = FullName,
                    Role = Role,
                    IsActive = true
                };

                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Role);
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            var users = await _userManager.Users.ToListAsync();
            return View("Index", users);
        }
    }
}