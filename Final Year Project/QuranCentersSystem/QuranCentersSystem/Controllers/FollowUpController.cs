using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace QuranCentersSystem.Controllers
{
    [Authorize] // حماية الصفحة لتتطلب تسجيل الدخول
    public class FollowUpController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FollowUpController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. الشاشة الرئيسية للمتابعة
        public async Task<IActionResult> Index()
        {
            // جلب الحلقات لعرضها في القائمة المنسدلة في أعلى الشاشة
            ViewBag.Circles = new SelectList(await _context.Circles.ToListAsync(), "Id", "Name");
            return View();
        }

        // 2. جلب طلاب الحلقة المختارة (AJAX) - يعمل عند تغيير القائمة المنسدلة
        [HttpGet]
        public async Task<IActionResult> GetStudents(int circleId)
        {
            var students = await _context.Students
                .Where(s => s.CircleId == circleId && s.Status == "نشط")
                .Select(s => new {
                    s.Id,
                    s.Name
                })
                .ToListAsync();

            return Json(students);
        }

        // 3. حفظ الإنجاز الفردي (AJAX) - (حفظ، مراجعة صغرى، كبرى)
        [HttpPost]
        public async Task<IActionResult> SaveAchievement([FromBody] StudentAchievement achievement)
        {
            if (achievement == null || achievement.StudentId == 0)
                return Json(new { success = false, message = "بيانات غير مكتملة" });

            try
            {
                if (achievement.Date == default) achievement.Date = DateTime.Today;

                _context.StudentAchievements.Add(achievement);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم تسجيل الإنجاز بنجاح!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // 4. حفظ الإنجاز الجماعي للحلقة (AJAX) - (تلقين، تجويد، تفسير)
        [HttpPost]
        public async Task<IActionResult> SaveGroupAchievement([FromBody] GroupAchievement groupAchievement)
        {
            if (groupAchievement == null || groupAchievement.CircleId == 0)
                return Json(new { success = false, message = "بيانات غير مكتملة" });

            try
            {
                if (groupAchievement.Date == default) groupAchievement.Date = DateTime.Today;

                _context.GroupAchievements.Add(groupAchievement);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "تم حفظ إنجاز الحلقة بنجاح!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ: " + ex.Message });
            }
        }

        // 5. حفظ الحضور والغياب (AJAX)
        [HttpPost]
        public async Task<IActionResult> SaveAttendance(int studentId, string status)
        {
            try
            {
                var attendance = new Attendance
                {
                    StudentId = studentId,
                    Date = DateTime.Today,
                    Status = status // حاضر، غائب، مستأذن
                };

                _context.Attendances.Add(attendance);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"تم تسجيل الطالب كـ: {status}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "حدث خطأ أثناء التحضير." });
            }
        }
    }
}