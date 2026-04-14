using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Services;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers.Api
{
    /// <summary>
    /// واجهة برمجة الطلاب - إدارة بيانات الطلاب وتوليد رموز QR
    /// محمية بمصادقة JWT
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class StudentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly QrCodeService _qrCodeService;

        public StudentsApiController(ApplicationDbContext context, QrCodeService qrCodeService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
        }

        /// <summary>
        /// جلب قائمة الطلاب مع إمكانية الفلترة حسب الحلقة
        /// GET: api/students?circleId=1
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStudents([FromQuery] int? circleId)
        {
            var query = _context.Students
                .Include(s => s.Circle)
                .AsQueryable();

            if (circleId.HasValue)
            {
                query = query.Where(s => s.CircleId == circleId.Value);
            }

            var students = await query.Select(s => new
            {
                s.Id,
                s.Name,
                s.Phone,
                s.Status,
                circleName = s.Circle != null ? s.Circle.Name : "غير محدد",
                joinDate = s.JoinDate.ToString("yyyy-MM-dd"),
                hasQrCode = !string.IsNullOrEmpty(s.QrCodeToken)
            }).ToListAsync();

            return Ok(students);
        }

        /// <summary>
        /// جلب بيانات طالب واحد مع تقدمه
        /// GET: api/students/{id}/progress
        /// </summary>
        [HttpGet("{id}/progress")]
        public async Task<IActionResult> GetStudentProgress(int id)
        {
            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (student == null)
                return NotFound(new { success = false, message = "الطالب غير موجود" });

            return Ok(new
            {
                student = new
                {
                    student.Id,
                    student.Name,
                    student.Phone,
                    circleName = student.Circle?.Name ?? "غير محدد",
                    student.Status,
                    joinDate = student.JoinDate.ToString("yyyy-MM-dd")
                },
                attendance = new
                {
                    total = student.Attendances.Count,
                    present = student.Attendances.Count(a => a.Status == "حاضر"),
                    absent = student.Attendances.Count(a => a.Status == "غائب"),
                    attendanceRate = student.Attendances.Count > 0
                        ? ((double)student.Attendances.Count(a => a.Status == "حاضر") / student.Attendances.Count * 100).ToString("F1") + "%"
                        : "N/A"
                },
                memorization = new
                {
                    totalSessions = student.Memorizations.Count,
                    lastSession = student.Memorizations
                        .OrderByDescending(m => m.Date)
                        .Select(m => new { m.SurahName, m.Grade, date = m.Date.ToString("yyyy-MM-dd") })
                        .FirstOrDefault(),
                    grades = student.Memorizations
                        .OrderByDescending(m => m.Date)
                        .Take(20)
                        .Select(m => new
                        {
                            date = m.Date.ToString("yyyy-MM-dd"),
                            m.SurahName,
                            m.Grade,
                            m.MistakesCount,
                            m.Type
                        })
                }
            });
        }

        /// <summary>
        /// توليد رمز QR لطالب معين
        /// GET: api/students/{id}/qrcode
        /// </summary>
        [HttpGet("{id}/qrcode")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Teacher")]
        public async Task<IActionResult> GenerateQrCode(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
                return NotFound(new { success = false, message = "الطالب غير موجود" });

            // توليد أو تحديث رمز QR
            var qrToken = _qrCodeService.GenerateQrToken(id);

            // حفظ الرمز في قاعدة البيانات
            student.QrCodeToken = qrToken;
            _context.Update(student);
            await _context.SaveChangesAsync();

            // توليد صورة QR
            var qrImageBytes = _qrCodeService.GenerateQrCodeImage(qrToken);

            return File(qrImageBytes, "image/png", $"qr_{student.Name}_{id}.png");
        }

        /// <summary>
        /// توليد رموز QR لجميع طلاب حلقة معينة
        /// POST: api/students/generate-qr-batch/{circleId}
        /// </summary>
        [HttpPost("generate-qr-batch/{circleId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
        public async Task<IActionResult> GenerateQrBatch(int circleId)
        {
            var students = await _context.Students
                .Where(s => s.CircleId == circleId)
                .ToListAsync();

            if (!students.Any())
                return NotFound(new { success = false, message = "لا يوجد طلاب في هذه الحلقة" });

            int generated = 0;
            foreach (var student in students)
            {
                if (string.IsNullOrEmpty(student.QrCodeToken))
                {
                    student.QrCodeToken = _qrCodeService.GenerateQrToken(student.Id);
                    generated++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"تم توليد {generated} رمز QR جديد",
                totalStudents = students.Count,
                newlyGenerated = generated
            });
        }
    }
}
