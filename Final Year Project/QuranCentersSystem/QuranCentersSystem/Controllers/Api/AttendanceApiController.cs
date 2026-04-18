using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using QuranCenters.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers.Api
{
    /// <summary>
    /// واجهة برمجة الحضور - تسجيل الحضور عبر QR Code وجلب بيانات الحضور
    /// محمية بمصادقة JWT
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class AttendanceApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IQrCodeService _qrCodeService;
        private readonly INotificationService _notificationService;

        public AttendanceApiController(
            ApplicationDbContext context,
            IQrCodeService qrCodeService,
            INotificationService notificationService)
        {
            _context = context;
            _qrCodeService = qrCodeService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// تسجيل حضور بمسح رمز QR
        /// POST: api/attendance/qr-scan
        /// </summary>
        [HttpPost("qr-scan")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Teacher")]
        public async Task<IActionResult> ScanQrAttendance([FromBody] QrScanRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.QrToken))
                return BadRequest(new { success = false, message = "رمز QR مطلوب" });

            // التحقق من صحة رمز QR
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.QrCodeToken == request.QrToken);

            if (student == null)
                return NotFound(new { success = false, message = "رمز QR غير صالح أو الطالب غير موجود" });

            // التحقق من عدم تسجيل حضور مكرر لنفس اليوم
            var today = DateTime.Today;
            var existingAttendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == student.Id && a.Date.Date == today);

            if (existingAttendance != null)
            {
                return Conflict(new
                {
                    success = false,
                    message = $"تم تسجيل حضور {student.Name} مسبقاً اليوم",
                    studentName = student.Name
                });
            }

            // تسجيل الحضور
            var attendance = new Attendance
            {
                StudentId = student.Id,
                Date = DateTime.Now,
                Status = "حاضر",
                Notes = "تم التسجيل عبر QR Code"
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            // إرسال إشعار لولي الأمر
            await _notificationService.NotifyParentOnAttendance(student.Id);

            return Ok(new
            {
                success = true,
                message = $"تم تسجيل حضور {student.Name} بنجاح",
                studentName = student.Name,
                studentId = student.Id,
                time = DateTime.Now.ToString("HH:mm")
            });
        }

        /// <summary>
        /// جلب حضور اليوم لحلقة معينة
        /// GET: api/attendance/today/{circleId}
        /// </summary>
        [HttpGet("today/{circleId}")]
        public async Task<IActionResult> GetTodayAttendance(int circleId)
        {
            var today = DateTime.Today;

            var students = await _context.Students
                .Where(s => s.CircleId == circleId)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    attendance = _context.Attendances
                        .Where(a => a.StudentId == s.Id && a.Date.Date == today)
                        .Select(a => new { a.Status, time = a.Date.ToString("HH:mm") })
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(new
            {
                date = today.ToString("yyyy-MM-dd"),
                circleId,
                students = students.Select(s => new
                {
                    s.Id,
                    s.Name,
                    status = s.attendance?.Status ?? "لم يسجل",
                    time = s.attendance?.time ?? "-"
                })
            });
        }

        /// <summary>
        /// تسجيل حضور يدوي لطالب
        /// POST: api/attendance/manual
        /// </summary>
        [HttpPost("manual")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin,Teacher")]
        public async Task<IActionResult> ManualAttendance([FromBody] ManualAttendanceRequest request)
        {
            if (request == null || request.StudentId <= 0)
                return BadRequest(new { success = false, message = "بيانات غير صالحة" });

            var student = await _context.Students.FindAsync(request.StudentId);
            if (student == null)
                return NotFound(new { success = false, message = "الطالب غير موجود" });

            var date = request.Date?.Date ?? DateTime.Today;
            var existing = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == request.StudentId && a.Date.Date == date);

            if (existing != null)
            {
                existing.Status = request.Status ?? "حاضر";
                existing.Notes = request.Notes ?? "";
            }
            else
            {
                _context.Attendances.Add(new Attendance
                {
                    StudentId = request.StudentId,
                    Date = date,
                    Status = request.Status ?? "حاضر",
                    Notes = request.Notes ?? ""
                });
            }

            await _context.SaveChangesAsync();

            // إرسال إشعار لولي الأمر
            await _notificationService.NotifyParentOnAttendance(request.StudentId);

            return Ok(new { success = true, message = "تم تسجيل الحضور بنجاح" });
        }
    }

    /// <summary>
    /// نموذج طلب مسح QR
    /// </summary>
    public class QrScanRequest
    {
        public string QrToken { get; set; }
    }

    /// <summary>
    /// نموذج طلب حضور يدوي
    /// </summary>
    public class ManualAttendanceRequest
    {
        public int StudentId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateTime? Date { get; set; }
    }
}
