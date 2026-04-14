using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Services
{
    /// <summary>
    /// خدمة الإشعارات - تُنشئ إشعارات داخلية آلية عند أحداث معينة
    /// مثل: تسجيل الحضور، إضافة مهمة جديدة، تسجيل درجة
    /// مصممة للتوسع لاحقاً مع Firebase Cloud Messaging (FCM) للإشعارات الفورية
    /// </summary>
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// إرسال إشعار لولي الأمر عند تسجيل حضور ابنه
        /// </summary>
        public async Task NotifyParentOnAttendance(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            // البحث عن حساب ولي الأمر عبر البريد الإلكتروني
            string parentEmail = student.Parent?.Email ?? student.ParentEmail;
            if (string.IsNullOrEmpty(parentEmail)) return;

            // البحث عن معرف المستخدم لولي الأمر في جدول Identity
            var parentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == parentEmail);

            if (parentUser == null) return;

            var notification = new Notification
            {
                UserId = parentUser.Id,
                Title = "تسجيل حضور",
                Message = $"تم تسجيل حضور ابنك/ابنتك {student.Name} اليوم {DateTime.Now:yyyy/MM/dd} الساعة {DateTime.Now:HH:mm}",
                Type = "Attendance",
                IsRead = false,
                CreatedAt = DateTime.Now,
                RelatedEntityId = studentId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// إرسال إشعار للطالب عند إضافة مهمة جديدة
        /// </summary>
        public async Task NotifyStudentOnNewTask(int studentId, string surahName)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return;

            // البحث عن حساب الطالب عبر بريده أو هاتفه
            string studentEmail = student.ParentEmail ?? student.Phone;
            if (string.IsNullOrEmpty(studentEmail)) return;

            var studentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == studentEmail);

            if (studentUser == null) return;

            var notification = new Notification
            {
                UserId = studentUser.Id,
                Title = "مهمة جديدة",
                Message = $"تم تسجيل مهمة حفظ جديدة في سورة {surahName}. راجع لوحة التحكم لمزيد من التفاصيل",
                Type = "Task",
                IsRead = false,
                CreatedAt = DateTime.Now,
                RelatedEntityId = studentId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// إرسال إشعار عند تسجيل درجة جديدة
        /// </summary>
        public async Task NotifyOnNewGrade(int studentId, string grade, string surahName)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            // إشعار لولي الأمر
            string parentEmail = student.Parent?.Email ?? student.ParentEmail;
            if (!string.IsNullOrEmpty(parentEmail))
            {
                var parentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == parentEmail);

                if (parentUser != null)
                {
                    _context.Notifications.Add(new Notification
                    {
                        UserId = parentUser.Id,
                        Title = "درجة جديدة",
                        Message = $"حصل {student.Name} على تقييم '{grade}' في سورة {surahName}",
                        Type = "Grade",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                        RelatedEntityId = studentId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// إرسال إشعار عام لمجموعة من المستخدمين
        /// </summary>
        public async Task SendBulkNotification(string[] userIds, string title, string message, string type)
        {
            foreach (var userId in userIds)
            {
                _context.Notifications.Add(new Notification
                {
                    UserId = userId,
                    Title = title,
                    Message = message,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
