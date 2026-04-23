using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    // ملاحظة: قمت بإضافة [Authorize] عامة وتخصيص الأدوار داخل الأكشنات
    [Authorize]
    public class ParentsController : Controller
    {
        private readonly IParentService _parentService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ParentsController(IParentService parentService, UserManager<ApplicationUser> userManager)
        {
            _parentService = parentService;
            _userManager = userManager;
        }

        // 🌟 أكشن إضافة ولي أمر جديد مع إنشاء حساب آلي (للمدير فقط)
        [Authorize(Roles = "Admin")]
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
                    await _parentService.CreateParentAsync(parent);
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
        [Authorize(Roles = "Admin,Parent")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var parentProfile = await _parentService.GetParentByEmailAsync(currentUser.Email);

            if (parentProfile == null)
            {
                return View("NoProfileFound");
            }

            var myChildren = await _parentService.GetChildrenAsync(parentProfile.Id);
            return View(myChildren);
        }

        [Authorize(Roles = "Admin,Parent")]
        public async Task<IActionResult> ChildDetails(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var roles = await _userManager.GetRolesAsync(currentUser);
            bool isAdmin = roles.Contains("Admin");

            // جلب ملف ولي الأمر
            var parentProfile = await _parentService.GetParentByEmailAsync(currentUser.Email);

            if (!isAdmin && parentProfile == null)
            {
                return Forbid();
            }

            Student student;
            if (isAdmin)
            {
                // المدير يمكنه رؤية أي طالب
                var studentService = HttpContext.RequestServices.GetService<IStudentService>();
                student = await studentService.GetStudentWithDetailsAsync(id);
            }
            else
            {
                // ولي الأمر يرى أبناءه فقط
                student = await _parentService.GetChildWithDetailsAsync(parentProfile.Id, id);
            }

            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }
    }
}