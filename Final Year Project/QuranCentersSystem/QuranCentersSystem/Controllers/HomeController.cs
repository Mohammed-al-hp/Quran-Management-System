using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCenters.Infrastructure.Identity;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// المتحكم الرئيسي - يوجه المستخدمين لوحة التحكم المناسبة حسب أدوارهم
    /// Admin: لوحة إحصائيات كاملة | Teacher: حلقاته وطلابه | Student: مهامه ودرجاته | Parent: تقدم أبنائه
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudentService _studentService;
        private readonly ITeacherService _teacherService;
        private readonly ICircleService _circleService;
        private readonly IMemorizationService _memorizationService;
        private readonly IAttendanceService _attendanceService;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            IStudentService studentService,
            ITeacherService teacherService,
            ICircleService circleService,
            IMemorizationService memorizationService,
            IAttendanceService attendanceService)
        {
            _userManager = userManager;
            _studentService = studentService;
            _teacherService = teacherService;
            _circleService = circleService;
            _memorizationService = memorizationService;
            _attendanceService = attendanceService;
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

            // لوحة تحكم المدير (الافتراضية)
            ViewBag.StudentsCount = await _studentService.GetStudentsCountAsync();
            ViewBag.TeachersCount = await _teacherService.GetTeachersCountAsync();
            ViewBag.CirclesCount = await _circleService.GetCirclesCountAsync();
            ViewBag.TodayAttendance = await _memorizationService.GetTodayActiveStudentsCountAsync();

            // بيانات الرسم البياني للتقييمات
            var grades = await _memorizationService.GetGradeDistributionAsync();
            ViewBag.ExcellentCount = grades.ExcellentCount;
            ViewBag.VeryGoodCount = grades.VeryGoodCount;
            ViewBag.GoodCount = grades.GoodCount;
            ViewBag.FairCount = grades.FairCount;

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

            var teacher = await _teacherService.GetTeacherByContactAsync(user.Email ?? user.UserName ?? "");

            if (teacher != null)
            {
                ViewBag.TeacherName = teacher.Name;
                ViewBag.CirclesCount = teacher.Circles.Count;
                ViewBag.StudentsCount = teacher.Circles.SelectMany(c => c.Students).Count();

                var studentIds = teacher.Circles.SelectMany(c => c.Students).Select(s => s.Id).ToList();
                var circleIds = teacher.Circles.Select(c => c.Id).ToList();

                ViewBag.TodayAttendance = await _attendanceService.GetTodayAttendanceCountAsync(circleIds);

                // آخر سجلات الحفظ
                ViewBag.RecentMemorizations = (await _memorizationService.GetRecentRecordsAsync(10))
                    .Where(m => studentIds.Contains(m.StudentId))
                    .ToList();
            }
            else
            {
                ViewBag.TeacherName = user.FullName ?? user.Email;
                ViewBag.CirclesCount = 0;
                ViewBag.StudentsCount = 0;
                ViewBag.TodayAttendance = 0;
                ViewBag.RecentMemorizations = new List<Memorization>();
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

            var student = await _studentService.GetStudentByEmailAsync(user.Email ?? "");

            if (student != null)
            {
                ViewBag.StudentName = student.Name;
                ViewBag.CircleName = student.Circle?.Name ?? "غير محدد";
                ViewBag.TotalAttendance = student.Attendances.Count;
                ViewBag.PresentCount = student.Attendances.Count(a => a.Status == "حاضر");
                ViewBag.AbsentCount = student.Attendances.Count(a => a.Status == "غائب");

                ViewBag.RecentGrades = student.Memorizations
                    .OrderByDescending(m => m.Date)
                    .Take(10)
                    .ToList();

                var gradeMap = new Dictionary<string, int>
                {
                    { "ممتاز", 4 }, { "جيد جداً", 3 }, { "جيد", 2 }, { "مقبول", 1 }
                };
                var gradeValues = student.Memorizations
                    .Where(m => gradeMap.ContainsKey(m.Grade ?? ""))
                    .Select(m => gradeMap[m.Grade!])
                    .ToList();
                ViewBag.AverageGrade = gradeValues.Any() ? gradeValues.Average().ToString("F1") : "N/A";

                // Gamification Info
                ViewBag.TotalPoints = student.PointsLedgers?.Sum(p => p.Points) ?? 0;
                ViewBag.StudentBadges = student.StudentBadges?.ToList() ?? new List<StudentBadge>();
            }
            else
            {
                ViewBag.StudentName = user.FullName ?? user.Email;
                ViewBag.CircleName = "غير محدد";
                ViewBag.TotalAttendance = 0;
                ViewBag.PresentCount = 0;
                ViewBag.AbsentCount = 0;
                ViewBag.RecentGrades = new List<Memorization>();
                ViewBag.AverageGrade = "N/A";
                ViewBag.TotalPoints = 0;
                ViewBag.StudentBadges = new List<StudentBadge>();
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