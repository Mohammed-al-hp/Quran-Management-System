using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System.Security.Claims;

namespace QuranCentersSystem.Controllers
{
    [Route("api/flutter")]
    [ApiController]
    public class FlutterApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public FlutterApiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET /api/flutter/student/me
        [HttpGet("student/me")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetMyStudentData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId ?? "");
            if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

            var student = await _context.Students
                .Include(s => s.Circle).ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.Username == user.UserName || s.ParentEmail == user.Email);

            if (student == null) return NotFound(new { message = "لم يتم العثور على بيانات الطالب" });

            var totalAttendance = await _context.Attendances.CountAsync(a => a.StudentId == student.Id);
            var presentCount = await _context.Attendances.CountAsync(a => a.StudentId == student.Id && a.Status == "حاضر");
            var absentCount = await _context.Attendances.CountAsync(a => a.StudentId == student.Id && a.Status == "غائب");
            var totalMemorization = await _context.StudentAchievements.CountAsync(a => a.StudentId == student.Id);

            var gradeStats = await _context.StudentAchievements
                .Where(a => a.StudentId == student.Id)
                .GroupBy(a => a.Grade)
                .Select(g => new { Grade = g.Key, Count = g.Count() })
                .ToListAsync();

            var recentMemorization = await _context.StudentAchievements
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.Date).Take(10)
                .Select(a => new { a.Id, Date = a.Date.ToString("yyyy-MM-dd"), Type = a.Type.ToString(), a.Grade, a.SurahStart, a.AyahStart, a.SurahEnd, a.AyahEnd, Notes = a.TeacherNotes })
                .ToListAsync();

            var recentAttendance = await _context.Attendances
                .Where(a => a.StudentId == student.Id)
                .OrderByDescending(a => a.Date).Take(30)
                .Select(a => new { a.Id, Date = a.Date.ToString("yyyy-MM-dd"), a.Status, a.Notes, a.DelayMinutes })
                .ToListAsync();

            return Ok(new
            {
                student = new
                {
                    student.Id,
                    student.Name,
                    student.Phone,
                    student.Status,
                    JoinDate = student.JoinDate.ToString("yyyy-MM-dd"),
                    student.ImagePath,
                    student.CurrentSurah,
                    circle = student.Circle == null ? null : new
                    {
                        student.Circle.Id,
                        student.Circle.Name,
                        student.Circle.Gender,
                        student.Circle.SelectedDays,
                        student.Circle.TimingType,
                        student.Circle.StartPrayer,
                        student.Circle.EndPrayer,
                        StartTime = student.Circle.StartTime.HasValue ? student.Circle.StartTime.Value.ToString(@"hh\:mm") : null,
                        teacher = student.Circle.Teacher == null ? null : new { student.Circle.Teacher.Id, student.Circle.Teacher.Name, student.Circle.Teacher.Phone }
                    }
                },
                attendance = new
                {
                    total = totalAttendance,
                    present = presentCount,
                    absent = absentCount,
                    attendanceRate = totalAttendance > 0 ? $"{Math.Round((double)presentCount / totalAttendance * 100, 1)}%" : "0%",
                    recent = recentAttendance
                },
                memorization = new { total = totalMemorization, grades = gradeStats, recent = recentMemorization }
            });
        }

        // GET /api/flutter/parent/me
        [HttpGet("parent/me")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetMyParentData()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId ?? "");
            if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

            var parent = await _context.Set<Parent>().FirstOrDefaultAsync(p => p.Email == user.Email);
            if (parent == null) return NotFound(new { message = "لم يتم العثور على بيانات ولي الأمر" });

            var children = await _context.Students
                .Where(s => s.ParentId == parent.Id)
                .Include(s => s.Circle).ThenInclude(c => c.Teacher)
                .ToListAsync();

            var childrenData = children.Select(s => new
            {
                s.Id,
                s.Name,
                s.Status,
                s.ImagePath,
                s.CurrentSurah,
                JoinDate = s.JoinDate.ToString("yyyy-MM-dd"),
                circle = s.Circle == null ? null : new
                {
                    s.Circle.Id,
                    s.Circle.Name,
                    s.Circle.SelectedDays,
                    s.Circle.TimingType,
                    s.Circle.StartPrayer,
                    teacher = s.Circle.Teacher == null ? null : new { s.Circle.Teacher.Name, s.Circle.Teacher.Phone }
                },
                attendanceRate = _context.Attendances.Count(a => a.StudentId == s.Id) > 0
                    ? Math.Round((double)_context.Attendances.Count(a => a.StudentId == s.Id && a.Status == "حاضر") / _context.Attendances.Count(a => a.StudentId == s.Id) * 100, 1) : 0.0,
                totalPresent = _context.Attendances.Count(a => a.StudentId == s.Id && a.Status == "حاضر"),
                totalAbsent = _context.Attendances.Count(a => a.StudentId == s.Id && a.Status == "غائب"),
                totalMemorization = _context.StudentAchievements.Count(a => a.StudentId == s.Id),
                lastGrade = _context.StudentAchievements.Where(a => a.StudentId == s.Id).OrderByDescending(a => a.Date).Select(a => a.Grade).FirstOrDefault()
            }).ToList();

            return Ok(new
            {
                parent = new { parent.Id, parent.Name, parent.Phone, parent.Email },
                children = childrenData
            });
        }

        // GET /api/flutter/parent/child/{studentId}
        [HttpGet("parent/child/{studentId}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetChildDetails(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Circle).ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return NotFound(new { message = "الطالب غير موجود" });

            var attendance = await _context.Attendances
                .Where(a => a.StudentId == studentId).OrderByDescending(a => a.Date).Take(30)
                .Select(a => new { Date = a.Date.ToString("yyyy-MM-dd"), a.Status, a.Notes })
                .ToListAsync();

            var memorization = await _context.StudentAchievements
                .Where(a => a.StudentId == studentId).OrderByDescending(a => a.Date).Take(20)
                .Select(a => new { Date = a.Date.ToString("yyyy-MM-dd"), Type = a.Type.ToString(), a.Grade, a.SurahStart, a.AyahStart, a.SurahEnd, a.AyahEnd, Notes = a.TeacherNotes })
                .ToListAsync();

            var presentCount = await _context.Attendances.CountAsync(a => a.StudentId == studentId && a.Status == "حاضر");
            var totalCount = await _context.Attendances.CountAsync(a => a.StudentId == studentId);

            return Ok(new
            {
                student = new
                {
                    student.Id,
                    student.Name,
                    student.Status,
                    student.CurrentSurah,
                    circle = student.Circle == null ? null : new { student.Circle.Name, student.Circle.SelectedDays, student.Circle.StartPrayer, teacher = student.Circle.Teacher?.Name }
                },
                attendanceSummary = new
                {
                    present = presentCount,
                    absent = totalCount - presentCount,
                    total = totalCount,
                    rate = totalCount > 0 ? $"{Math.Round((double)presentCount / totalCount * 100, 1)}%" : "0%"
                },
                attendance,
                memorization
            });
        }

        // GET /api/flutter/student/{studentId}/notifications
        [HttpGet("student/{studentId}/notifications")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetStudentNotifications(int studentId)
        {
            var notifications = new List<object>();

            var absences = await _context.Attendances
                .Where(a => a.StudentId == studentId && a.Status == "غائب")
                .OrderByDescending(a => a.Date).Take(5).ToListAsync();

            foreach (var abs in absences)
                notifications.Add(new { type = "absence", emoji = "⚠️", title = "تسجيل غياب", body = $"تم تسجيل غيابك بتاريخ {abs.Date:yyyy-MM-dd}", date = abs.Date.ToString("yyyy-MM-dd") });

            var grades = await _context.StudentAchievements
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date).Take(5).ToListAsync();

            foreach (var grade in grades)
                notifications.Add(new { type = "grade", emoji = grade.Grade == "ممتاز" ? "⭐" : grade.Grade == "جيد جداً" ? "👍" : "📖", title = $"تقييم {grade.Grade}", body = $"حصلت على تقييم {grade.Grade} في {grade.Type}", date = grade.Date.ToString("yyyy-MM-dd") });

            return Ok(notifications.OrderByDescending(n => ((dynamic)n).date).Take(15));
        }

        // GET /api/flutter/student/{studentId}/schedule
        [HttpGet("student/{studentId}/schedule")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        public async Task<IActionResult> GetSchedule(int studentId)
        {
            var student = await _context.Students.Include(s => s.Circle).FirstOrDefaultAsync(s => s.Id == studentId);
            if (student?.Circle == null) return NotFound(new { message = "لا توجد حلقة مرتبطة" });

            return Ok(new
            {
                circleName = student.Circle.Name,
                days = student.Circle.SelectedDays,
                timingType = student.Circle.TimingType,
                startPrayer = student.Circle.StartPrayer,
                endPrayer = student.Circle.EndPrayer,
                startTime = student.Circle.StartTime.HasValue ? student.Circle.StartTime.Value.ToString(@"hh\:mm") : null,
                endTime = student.Circle.EndTime.HasValue ? student.Circle.EndTime.Value.ToString(@"hh\:mm") : null
            });
        }
    }
}