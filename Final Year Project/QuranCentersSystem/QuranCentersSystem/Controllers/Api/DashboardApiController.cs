using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers.Api
{
    /// <summary>
    /// واجهة برمجة لوحة التحكم - توفر إحصائيات ورسوم بيانية وتحليلات ذكية
    /// محمية بمصادقة JWT للاستخدام من تطبيق Flutter
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DashboardApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// جلب الإحصائيات العامة للنظام
        /// GET: api/dashboard/stats
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var today = DateTime.Today;
            var stats = new
            {
                studentsCount = await _context.Students.CountAsync(),
                teachersCount = await _context.Teachers.CountAsync(),
                circlesCount = await _context.Circles.CountAsync(),
                todayAttendance = await _context.Attendances
                    .Where(a => a.Date.Date == today && a.Status == "حاضر")
                    .CountAsync(),
                totalMemorizations = await _context.Memorizations.CountAsync(),
                maxCapacity = 52
            };

            return Ok(stats);
        }

        /// <summary>
        /// تحليلات ذكية - الطلاب المتفوقون واتجاهات الحضور
        /// GET: api/dashboard/analytics
        /// </summary>
        [HttpGet("analytics")]
        public async Task<IActionResult> GetAnalytics()
        {
            // الطلاب الأكثر تفوقاً (أكبر عدد تقييمات ممتاز)
            var topStudents = await _context.Memorizations
                .Where(m => m.Grade == "ممتاز")
                .GroupBy(m => m.StudentId)
                .Select(g => new
                {
                    studentId = g.Key,
                    excellentCount = g.Count()
                })
                .OrderByDescending(x => x.excellentCount)
                .Take(5)
                .ToListAsync();

            // جلب أسماء الطلاب المتفوقين
            var studentIds = topStudents.Select(t => t.studentId).ToList();
            var students = await _context.Students
                .Where(s => studentIds.Contains(s.Id))
                .Select(s => new { s.Id, s.Name })
                .ToListAsync();

            var topStudentsWithNames = topStudents.Select(t => new
            {
                t.studentId,
                studentName = students.FirstOrDefault(s => s.Id == t.studentId)?.Name ?? "غير معروف",
                t.excellentCount
            }).ToList();

            // اتجاهات الحضور لآخر 7 أيام
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .ToList();

            var attendanceTrends = new System.Collections.Generic.List<object>();
            foreach (var day in last7Days)
            {
                var presentCount = await _context.Attendances
                    .Where(a => a.Date.Date == day.Date && a.Status == "حاضر")
                    .CountAsync();
                var absentCount = await _context.Attendances
                    .Where(a => a.Date.Date == day.Date && a.Status == "غائب")
                    .CountAsync();

                attendanceTrends.Add(new
                {
                    date = day.ToString("yyyy-MM-dd"),
                    dayName = day.ToString("dddd", new System.Globalization.CultureInfo("ar-SA")),
                    present = presentCount,
                    absent = absentCount
                });
            }

            // توزيع التقييمات
            var gradeDistribution = new
            {
                excellent = await _context.Memorizations.CountAsync(m => m.Grade == "ممتاز"),
                veryGood = await _context.Memorizations.CountAsync(m => m.Grade == "جيد جداً"),
                good = await _context.Memorizations.CountAsync(m => m.Grade == "جيد"),
                fair = await _context.Memorizations.CountAsync(m => m.Grade == "مقبول")
            };

            return Ok(new
            {
                topStudents = topStudentsWithNames,
                attendanceTrends,
                gradeDistribution
            });
        }

        /// <summary>
        /// تقدم طالب معين - حضوره ودرجاته
        /// GET: api/dashboard/student/{id}
        /// </summary>
        [HttpGet("student/{id}")]
        public async Task<IActionResult> GetStudentProgress(int id)
        {
            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound(new { success = false, message = "الطالب غير موجود" });

            var progress = new
            {
                student = new
                {
                    student.Id,
                    student.Name,
                    circleName = student.Circle?.Name ?? "غير محدد",
                    student.Status,
                    joinDate = student.JoinDate.ToString("yyyy-MM-dd")
                },
                attendance = new
                {
                    total = student.Attendances.Count,
                    present = student.Attendances.Count(a => a.Status == "حاضر"),
                    absent = student.Attendances.Count(a => a.Status == "غائب"),
                    late = student.Attendances.Count(a => a.Status == "متأخر"),
                    excused = student.Attendances.Count(a => a.Status == "مستأذن")
                },
                memorization = new
                {
                    total = student.Memorizations.Count,
                    excellent = student.Memorizations.Count(m => m.Grade == "ممتاز"),
                    veryGood = student.Memorizations.Count(m => m.Grade == "جيد جداً"),
                    good = student.Memorizations.Count(m => m.Grade == "جيد"),
                    fair = student.Memorizations.Count(m => m.Grade == "مقبول"),
                    recentRecords = student.Memorizations
                        .OrderByDescending(m => m.Date)
                        .Take(10)
                        .Select(m => new
                        {
                            date = m.Date.ToString("yyyy-MM-dd"),
                            m.SurahName,
                            m.Grade,
                            m.MistakesCount,
                            m.Type
                        })
                }
            };

            return Ok(progress);
        }
    }
}
