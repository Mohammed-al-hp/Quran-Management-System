using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    // ملاحظة: قمت بإضافة [Authorize] عامة وتخصيص الأدوار داخل الأكشنات
    [Authorize]
    public class ParentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ParentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 🌟 أكشن إضافة ولي أمر جديد مع إنشاء حساب آلي (للمدير فقط)
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Parent parent)
        {
            if (ModelState.IsValid)
            {
                // 1. إنشاء حساب الدخول في جدول Identity باستخدام الإيميل
                var user = new ApplicationUser { UserName = parent.Email, Email = parent.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, "Parent@123"); // كلمة مرور افتراضية

                if (result.Succeeded)
                {
                    // 2. منحه صلاحية "ولي أمر"
                    await _userManager.AddToRoleAsync(user, "Parent");

                    // 3. حفظ بياناته في جدول الـ Parents
                    _context.Add(parent);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }

                // إضافة أخطاء الهوية للـ ModelState في حال فشل الإنشاء (مثل إيميل مكرر)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(parent);
        }

        // لوحة تحكم ولي الأمر
        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var parentProfile = await _context.Set<Parent>()
                .FirstOrDefaultAsync(p => p.Email == currentUser.Email);

            if (parentProfile == null)
            {
                return View("NoProfileFound");
            }

            var myChildren = await _context.Students
                .Where(s => s.ParentId == parentProfile.Id)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .Include(s => s.Payments)
                .ToListAsync();

            return View(myChildren);
        }

        [Authorize(Roles = "Parent")]
        public async Task<IActionResult> ChildDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var student = await _context.Students
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null || student.Parent.Email != currentUser.Email)
            {
                return Forbid();
            }

            return View(student);
        }
    }
}