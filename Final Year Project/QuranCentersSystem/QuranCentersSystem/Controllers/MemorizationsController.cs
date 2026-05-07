using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuranCentersSystem.Controllers
{
    [Authorize]
    public class MemorizationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MemorizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. العرض الرئيسي للمتابعة اليومية
        public async Task<IActionResult> Index(int? circleId, DateTime? date)
        {
            var targetDate = date ?? DateTime.Today;
            ViewBag.SelectedDate = targetDate;
            ViewBag.Circles = await _context.Circles.ToListAsync();

            var query = _context.Students
                .Where(s => s.Status == "نشط")
                .AsQueryable();

            if (circleId.HasValue)
            {
                query = query.Where(s => s.CircleId == circleId);
                ViewBag.CurrentCircleId = circleId;
            }

            var students = await query.Select(s => new StudentDailyViewModel
            {
                StudentId = s.Id,
                StudentName = s.Name,
                // استخدام موديل StudentAchievement الجديد
                LastAchievement = _context.StudentAchievements
                    .Where(m => m.StudentId == s.Id)
                    .OrderByDescending(m => m.Date)
                    .FirstOrDefault(),
                // استخدام موديل Attendance
                IsPresent = _context.Attendances
                    .Any(a => a.StudentId == s.Id && a.Date.Date == targetDate.Date)
            }).ToListAsync();

            return View(students);
        }

        // 2. حفظ إنجاز التسميع (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRecord([FromBody] StudentAchievement record)
        {
            if (record == null || record.StudentId <= 0)
                return Json(new { success = false, message = "بيانات غير مكتملة" });

            try
            {
                if (record.Date == default) record.Date = DateTime.Today;

                _context.StudentAchievements.Add(record);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم تسجيل الإنجاز بنجاح" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "خطأ في الخادم: " + ex.Message });
            }
        }

        // 3. جلب آخر بيانات الطالب تلقائياً لتسهيل الإدخال
        [HttpGet]
        public async Task<IActionResult> GetStudentContext(int studentId)
        {
            var lastRecord = await _context.StudentAchievements
                .Where(m => m.StudentId == studentId)
                .OrderByDescending(m => m.Date)
                .Select(m => new {
                    m.SurahEnd,
                    m.AyahEnd,
                    m.Type
                })
                .FirstOrDefaultAsync();

            return Json(lastRecord);
        }

        // 4. تحديث حالة الحضور السريع
        [HttpPost]
        public async Task<IActionResult> UpdateAttendance(int studentId, bool isPresent)
        {
            var date = DateTime.Today;
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == date);

            if (isPresent)
            {
                if (attendance == null)
                {
                    _context.Attendances.Add(new Attendance
                    {
                        StudentId = studentId,
                        Date = date,
                        Status = "حاضر" //
                    });
                }
            }
            else if (attendance != null)
            {
                _context.Remove(attendance);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // 5. حفظ الإنجاز الجماعي للحلقة
        [HttpPost]
        public async Task<IActionResult> SaveGroupAchievement([FromBody] GroupAchievement groupRecord)
        {
            if (groupRecord == null) return Json(new { success = false });

            _context.GroupAchievements.Add(groupRecord); //[cite: 1]
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }

    public class StudentDailyViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public StudentAchievement LastAchievement { get; set; }
        public bool IsPresent { get; set; }
    }
}