using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize] // تأكد من وجود هذا السطر لحماية الصفحة
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // جلب الإحصائيات من قاعدة البيانات
            ViewBag.StudentsCount = await _context.Students.CountAsync();
            ViewBag.TeachersCount = await _context.Teachers.CountAsync();
            ViewBag.CirclesCount = await _context.Circles.CountAsync();

            // جلب حضور اليوم (كمثال بسيط: عدد السجلات المسجلة اليوم في جدول Memorization)
            ViewBag.TodayAttendance = await _context.Memorizations
                .Where(m => m.Date.Date == System.DateTime.Today)
                .Select(m => m.StudentId)
                .Distinct()
                .CountAsync();

            return View();
        }
    }
}