using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Controllers
{
    public class CirclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CirclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض قائمة الحلقات
        public IActionResult Index()
        {
            var circles = _context.Circles
                .Include(c => c.Teacher)
                .ToList();
            return View(circles);
        }

        // 2. عرض صفحة إضافة حلقة جديدة
        public IActionResult Create()
        {
            // جلب قائمة المعلمين لعرضهم في قائمة منسدلة (Dropdown)
            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name");
            return View();
        }

        // 3. حفظ بيانات الحلقة الجديدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,TeacherId")] Circle circle)
        {
            // أضف هذه الأسطر لإخبار السيرفر بأن هذه الحقول ليست ضرورية للتحقق الآن
            ModelState.Remove("Teacher");
            ModelState.Remove("Students"); // إذا كان هناك قائمة طلاب

            if (ModelState.IsValid)
            {
                // تعيين قيم افتراضية للحقول الجديدة حتى لا يرفضها SQL Server
                // circle.CurrentStudents = 0; 
                // circle.MaxCapacity = 52;

                _context.Add(circle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name", circle.TeacherId);
            return View(circle);
        }
    }
}