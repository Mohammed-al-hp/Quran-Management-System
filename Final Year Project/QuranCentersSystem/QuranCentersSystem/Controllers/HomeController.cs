using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // جلب الإحصائيات الأساسية
            ViewBag.StudentsCount = await _context.Students.CountAsync();
            ViewBag.TeachersCount = await _context.Teachers.CountAsync();
            ViewBag.CirclesCount = await _context.Circles.CountAsync();

            // حساب حضور اليوم (عدد الطلاب الذين سجلوا إنجازاً اليوم)
            ViewBag.TodayAttendance = await _context.Memorizations
                .Where(m => m.Date.Date == System.DateTime.Today)
                .Select(m => m.StudentId)
                .Distinct()
                .CountAsync();

            // تجهيز بيانات الرسم البياني للتقييمات
            ViewBag.ExcellentCount = await _context.Memorizations.CountAsync(m => m.Grade == "ممتاز");
            ViewBag.VeryGoodCount = await _context.Memorizations.CountAsync(m => m.Grade == "جيد جداً");
            ViewBag.GoodCount = await _context.Memorizations.CountAsync(m => m.Grade == "جيد");
            ViewBag.FairCount = await _context.Memorizations.CountAsync(m => m.Grade == "مقبول");

            return View();
        }
    }
}