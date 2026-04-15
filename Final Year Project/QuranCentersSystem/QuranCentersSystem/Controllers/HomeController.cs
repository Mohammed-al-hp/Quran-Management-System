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

            // --- 1. إحصائيات المدير (Admin) ---
            if (User.IsInRole("Admin"))
            {
                ViewBag.StudentsCount = await _context.Students.CountAsync();
                ViewBag.TeachersCount = await _context.Teachers.CountAsync();
                ViewBag.CirclesCount = await _context.Circles.CountAsync();
                ViewBag.TodayAttendance = await GetTodayAttendanceCount(null);
            }
            // --- 2. إحصائيات المحفظ (Teacher) ---
            else if (User.IsInRole("Teacher"))
            {
                // نربط المحفظ بحلقاته عن طريق البريد الإلكتروني
                var teacher = await _context.Teachers
                    .Include(t => t.Circles)
                    .FirstOrDefaultAsync(t => t.Phone == user.PhoneNumber || t.Name == user.UserName); // تخصيص الربط حسب موديلك

                if (teacher != null)
                {
                    var circleIds = teacher.Circles.Select(c => c.Id).ToList();
                    ViewBag.StudentsCount = await _context.Students.CountAsync(s => circleIds.Contains(s.CircleId));
                    ViewBag.CirclesCount = teacher.Circles.Count;
                    ViewBag.TodayAttendance = await GetTodayAttendanceCount(circleIds);
                }
            }
            // --- 3. إحصائيات ولي الأمر (Parent) ---
            else if (User.IsInRole("Parent"))
            {
                ViewBag.MyStudentsCount = await _context.Students.CountAsync(s => s.ParentEmail == user.Email);
            }

            // بيانات الرسم البياني (عمة للمركز أو مخصصة)
            await PrepareGradesChartData();

            return View();
        }

        private async Task<int> GetTodayAttendanceCount(List<int> circleIds)
        {
            var query = _context.Memorizations.Where(m => m.Date.Date == System.DateTime.Today);
            if (circleIds != null)
            {
                query = query.Where(m => circleIds.Contains(m.Student.CircleId));
            }
            return await query.Select(m => m.StudentId).Distinct().CountAsync();
        }

        private async Task PrepareGradesChartData()
        {
            ViewBag.ExcellentCount = await _context.Memorizations.CountAsync(m => m.Grade == "ممتاز");
            ViewBag.VeryGoodCount = await _context.Memorizations.CountAsync(m => m.Grade == "جيد جداً");
            ViewBag.GoodCount = await _context.Memorizations.CountAsync(m => m.Grade == "جيد");
            ViewBag.FairCount = await _context.Memorizations.CountAsync(m => m.Grade == "مقبول");
        }
    }
}