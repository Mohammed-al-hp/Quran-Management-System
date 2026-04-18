using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;
using QuranCenters.Core.Entities;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// المتحكم الرئيسي - يوجه المستخدمين لوحة التحكم المناسبة حسب أدوارهم
    /// Admin: لوحة إحصائيات كاملة | Teacher: حلقاته وطلابه | Student: مهامه ودرجاته | Parent: تقدم أبنائه
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// الصفحة الرئيسية - تقوم بتوجيه المستخدم حسب دوره
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            // توجيه حسب الدور
            switch (role)
            {
                case "Teacher":
                    return RedirectToAction(nameof(TeacherDashboard));
                case "Student":
                    return RedirectToAction(nameof(StudentDashboard));
                case "Parent":
                    return RedirectToAction("Index", "Parents");
                default: // Admin أو بدون دور
                    break;
            }

            // لوحة تحكم المدير (الافتراضية) - لا تغيير في الـ View
            ViewBag.StudentsCount = await _context.Students.CountAsync();
            ViewBag.TeachersCount = await _context.Teachers.CountAsync();
            ViewBag.CirclesCount = await _context.Circles.CountAsync();

            // حساب حضور اليوم
            ViewBag.TodayAttendance = await _context.Memorizations
                .Where(m => m.Date.Date == System.DateTime.Today)
                .Select(m => m.StudentId)
                .Distinct()
                .CountAsync();

            // بيانات الرسم البياني للتقييمات
            ViewBag.ExcellentCount = await _context.Memorizations.CountAsync(m => m.Grade == "ممتاز");
            ViewBag.VeryGoodCount = await _context.Memorizations.CountAsync(m => m.Grade == "جيد جداً");
            ViewBag.GoodCount = await _context.Memorizations.CountAsync(m => m.Grade == "جيد");
            ViewBag.FairCount = await _context.Memorizations.CountAsync(m => m.Grade == "مقبول");

            return View();
        }

        /// <summary>
        /// لوحة تحكم المحفظ - تعرض حلقاته وإحصائيات طلابه
        /// </summary>
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> TeacherDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // البحث عن ملف المحفظ المرتبط بالبريد الإلكتروني
            var teacher = await _context.Teachers
                .Include(t => t.Circles)
                    .ThenInclude(c => c.Students)
                .FirstOrDefaultAsync(t => t.Phone == user.Email || t.Name.Contains(user.UserName ?? ""));

            if (teacher != null)
            {
                ViewBag.TeacherName = teacher.Name;
                ViewBag.CirclesCount = teacher.Circles.Count;
                ViewBag.StudentsCount = teacher.Circles.SelectMany(c => c.Students).Count();

                // حضور اليوم لطلابه
                var studentIds = teacher.Circles.SelectMany(c => c.Students).Select(s => s.Id).ToList();
                ViewBag.TodayAttendance = await _context.Attendances
                    .Where(a => studentIds.Contains(a.StudentId) && a.Date.Date == System.DateTime.Today)
                    .CountAsync();

                // آخر سجلات الحفظ
                ViewBag.RecentMemorizations = await _context.Memorizations
                    .Include(m => m.Student)
                    .Where(m => studentIds.Contains(m.StudentId))
                    .OrderByDescending(m => m.Date)
                    .Take(10)
                    .ToListAsync();
            }
            else
            {
                ViewBag.TeacherName = user.Email;
                ViewBag.CirclesCount = 0;
                ViewBag.StudentsCount = 0;
                ViewBag.TodayAttendance = 0;
                ViewBag.RecentMemorizations = new System.Collections.Generic.List<QuranCenters.Core.Entities.Memorization>();
            }

            return View();
        }

        /// <summary>
        /// لوحة تحكم الطالب - تعرض مهامه اليومية ودرجاته وحضوره
        /// </summary>
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> StudentDashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // البحث عن ملف الطالب المرتبط بالبريد
            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .FirstOrDefaultAsync(s => s.ParentEmail == user.Email || s.Phone == user.Email);

            if (student != null)
            {
                ViewBag.StudentName = student.Name;
                ViewBag.CircleName = student.Circle?.Name ?? "غير محدد";
                ViewBag.TotalAttendance = student.Attendances.Count;
                ViewBag.PresentCount = student.Attendances.Count(a => a.Status == "حاضر");
                ViewBag.AbsentCount = student.Attendances.Count(a => a.Status == "غائب");

                // آخر الدرجات
                ViewBag.RecentGrades = student.Memorizations
                    .OrderByDescending(m => m.Date)
                    .Take(10)
                    .ToList();

                // متوسط التقييم
                var gradeMap = new System.Collections.Generic.Dictionary<string, int>
                {
                    { "ممتاز", 4 }, { "جيد جداً", 3 }, { "جيد", 2 }, { "مقبول", 1 }
                };
                var grades = student.Memorizations
                    .Where(m => gradeMap.ContainsKey(m.Grade ?? ""))
                    .Select(m => gradeMap[m.Grade!])
                    .ToList();
                ViewBag.AverageGrade = grades.Any() ? grades.Average().ToString("F1") : "N/A";

                // Gamification Info
                ViewBag.TotalPoints = await _context.PointsLedgers.Where(p => p.StudentId == student.Id).SumAsync(p => p.Points);
                ViewBag.StudentBadges = await _context.StudentBadges.Where(b => b.StudentId == student.Id).ToListAsync();
            }
            else
            {
                ViewBag.StudentName = user.Email;
                ViewBag.CircleName = "غير محدد";
                ViewBag.TotalAttendance = 0;
                ViewBag.PresentCount = 0;
                ViewBag.AbsentCount = 0;
                ViewBag.RecentGrades = new System.Collections.Generic.List<Memorization>();
                ViewBag.AverageGrade = "N/A";
                ViewBag.TotalPoints = 0;
                ViewBag.StudentBadges = new System.Collections.Generic.List<StudentBadge>();
            }

            return View();
        }

        /// <summary>
        /// صفحة الخطأ
        /// </summary>
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new QuranCentersSystem.Models.ErrorViewModel
            {
                RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}