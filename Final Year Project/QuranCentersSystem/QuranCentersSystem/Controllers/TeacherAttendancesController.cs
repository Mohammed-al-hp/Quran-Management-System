using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models; // هذا السطر هو مفتاح الحل لإصلاح خطأ CS1061

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeacherAttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeacherAttendancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // عرض القائمة الشاملة لجميع سجلات حضور المعلمين
        public async Task<IActionResult> Index()
        {
            var list = await _context.TeacherAttendances
                .Include(t => t.Teacher)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
            return View(list);
        }

        // شاشة الحضور الجماعي للمعلمين (التحضير اليومي)
        public async Task<IActionResult> Manage()
        {
            var teachers = await _context.Teachers.ToListAsync();
            ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
            return View(teachers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveAttendance(DateTime date, Dictionary<int, string> status, Dictionary<int, double> hours, Dictionary<int, string> notes)
        {
            if (status == null) return BadRequest("لم يتم إرسال بيانات الحضور");

            foreach (var teacherId in status.Keys)
            {
                var existing = await _context.TeacherAttendances
                    .FirstOrDefaultAsync(a => a.TeacherId == teacherId && a.Date.Date == date.Date);

                if (existing != null)
                {
                    // تحديث السجل الموجود مسبقاً
                    existing.Status = status[teacherId];
                    existing.WorkHours = hours.ContainsKey(teacherId) ? hours[teacherId] : 0;
                    existing.Notes = notes.ContainsKey(teacherId) ? notes[teacherId] : "";
                }
                else
                {
                    // إضافة سجل حضور جديد
                    _context.TeacherAttendances.Add(new TeacherAttendance
                    {
                        TeacherId = teacherId,
                        Date = date,
                        Status = status[teacherId],
                        WorkHours = hours.ContainsKey(teacherId) ? hours[teacherId] : 0,
                        Notes = notes.ContainsKey(teacherId) ? notes[teacherId] : ""
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}