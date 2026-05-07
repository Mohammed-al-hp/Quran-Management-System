using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // إنشاء الكائن الذي سنرسله للشاشة
            var model = new DashboardViewModel();

            // --- 1. إحصائيات المدير (Admin) ---
            if (User.IsInRole("Admin"))
            {
                model.TotalStudents = await _context.Students.CountAsync();
                model.TotalTeachers = await _context.Teachers.CountAsync();
                model.TotalCircles = await _context.Circles.CountAsync();
                model.TodayAttendance = await GetTodayAttendanceCount(null);
            }
            // --- 2. إحصائيات المحفظ (Teacher) ---
            else if (User.IsInRole("Teacher"))
            {
                var teacher = await _context.Teachers
                    .Include(t => t.Circles)
                    .FirstOrDefaultAsync(t => t.Phone == user.PhoneNumber || t.Name == user.UserName);

                if (teacher != null)
                {
                    var circleIds = teacher.Circles.Select(c => c.Id).ToList();
                    model.TotalStudents = await _context.Students.CountAsync(s => circleIds.Contains(s.CircleId));
                    model.TotalCircles = teacher.Circles.Count;
                    model.TodayAttendance = await GetTodayAttendanceCount(circleIds);
                    model.TotalTeachers = 1; // المحفظ يرى نفسه فقط
                }
            }
            // --- 3. إحصائيات ولي الأمر (Parent) ---
            else if (User.IsInRole("Parent"))
            {
                model.TotalStudents = await _context.Students.CountAsync(s => s.ParentEmail == user.Email);
            }

            // تعبئة بيانات الرسم البياني داخل الموديل
            model.ExcellentCount = await _context.StudentAchievements.CountAsync(m => m.Grade == "ممتاز");
            model.VeryGoodCount = await _context.StudentAchievements.CountAsync(m => m.Grade == "جيد جداً");
            model.GoodCount = await _context.StudentAchievements.CountAsync(m => m.Grade == "جيد");
            model.FairCount = await _context.StudentAchievements.CountAsync(m => m.Grade == "مقبول");

            // إرسال الموديل بالكامل إلى الشاشة
            return View(model);
        }

        private async Task<int> GetTodayAttendanceCount(List<int>? circleIds)
        {
            var query = _context.StudentAchievements.Where(m => m.Date.Date == System.DateTime.Today);
            if (circleIds != null && circleIds.Any())
            {
                query = query.Where(m => circleIds.Contains(m.Student.CircleId));
            }
            return await query.Select(m => m.StudentId).Distinct().CountAsync();
        }
    }
}