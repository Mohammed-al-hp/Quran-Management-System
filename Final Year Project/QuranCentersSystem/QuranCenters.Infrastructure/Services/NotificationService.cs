using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using QuranCenters.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using QuranCenters.Infrastructure.Hubs;

namespace QuranCenters.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        private async Task SendRealTimeNotification(string userId, string title, string message)
        {
            try
            {
                await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", new { title, message });
            }
            catch (Exception)
            {
                // Simple logging or ignore if no clients connected
            }
        }

        public async Task NotifyParentOnAttendance(int studentId)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            string parentEmail = student.Parent?.Email ?? student.ParentEmail;
            if (string.IsNullOrEmpty(parentEmail)) return;

            var parentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == parentEmail);

            if (parentUser == null) return;

            var title = "تسجيل حضور";
            var message = $"{student.Name} حضر الحلقة الآن بنشاط، بارك الله في حرصكم.";

            var notification = new Notification
            {
                UserId = parentUser.Id,
                Title = title,
                Message = message,
                Type = "Attendance",
                IsRead = false,
                CreatedAt = DateTime.Now,
                RelatedEntityId = studentId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendRealTimeNotification(parentUser.Id, title, message);
        }

        public async Task NotifyStudentOnNewTask(int studentId, string surahName)
        {
            var student = await _context.Students.FindAsync(studentId);
            if (student == null) return;

            string studentEmail = student.ParentEmail ?? student.Phone;
            if (string.IsNullOrEmpty(studentEmail)) return;

            var studentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == studentEmail);

            if (studentUser == null) return;

            var title = "مهمة جديدة";
            var message = $"تم تسجيل مهمة حفظ جديدة في سورة {surahName}. راجع لوحة التحكم لمزيد من التفاصيل";

            var notification = new Notification
            {
                UserId = studentUser.Id,
                Title = title,
                Message = message,
                Type = "Task",
                IsRead = false,
                CreatedAt = DateTime.Now,
                RelatedEntityId = studentId
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            await SendRealTimeNotification(studentUser.Id, title, message);
        }

        public async Task NotifyOnNewGrade(int studentId, string grade, string surahName)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            string parentEmail = student.Parent?.Email ?? student.ParentEmail;
            if (!string.IsNullOrEmpty(parentEmail))
            {
                var parentUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == parentEmail);

                if (parentUser != null)
                {
                    var title = "درجة جديدة";
                    var message = $"حصل {student.Name} على تقييم '{grade}' في سورة {surahName}";

                    _context.Notifications.Add(new Notification
                    {
                        UserId = parentUser.Id,
                        Title = title,
                        Message = message,
                        Type = "Grade",
                        IsRead = false,
                        CreatedAt = DateTime.Now,
                        RelatedEntityId = studentId
                    });

                    await _context.SaveChangesAsync();
                    await SendRealTimeNotification(parentUser.Id, title, message);
                }
            }
        }

        public async Task NotifyParentOnNewBadge(int studentId, string badgeName)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            string parentEmail = student.Parent?.Email ?? student.ParentEmail;
            if (string.IsNullOrEmpty(parentEmail)) return;

            var parentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == parentEmail);

            if (parentUser == null) return;

            var title = "وسام جديد! 🏆";
            var message = $"تهانينا! حصل بطلنا {student.Name} على وسام [{badgeName}] الجديد! 🏆 استمر في التألق.";

            _context.Notifications.Add(new Notification
            {
                UserId = parentUser.Id,
                Title = title,
                Message = message,
                Type = "Badge",
                IsRead = false,
                CreatedAt = DateTime.Now,
                RelatedEntityId = studentId
            });

            await _context.SaveChangesAsync();
            await SendRealTimeNotification(parentUser.Id, title, message);
        }

        public async Task NotifyParentOnPointsAwarded(int studentId, int points, string reason)
        {
            var student = await _context.Students
                .Include(s => s.Parent)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return;

            string parentEmail = student.Parent?.Email ?? student.ParentEmail;
            if (string.IsNullOrEmpty(parentEmail)) return;

            var parentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == parentEmail);

            if (parentUser == null) return;

            var title = "نقاط جديدة 🌟";
            var message = $"ما شاء الله! زاد رصيد {student.Name} بمقدار {points} نقطة لتميزه اليوم! 🌟";

            _context.Notifications.Add(new Notification
            {
                UserId = parentUser.Id,
                Title = title,
                Message = message,
                Type = "Points",
                IsRead = false,
                CreatedAt = DateTime.Now,
                RelatedEntityId = studentId
            });

            await _context.SaveChangesAsync();
            await SendRealTimeNotification(parentUser.Id, title, message);
        }

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

                await SendRealTimeNotification(userId, title, message);
            }

            await _context.SaveChangesAsync();
        }

        // --- إدارة الإشعارات (جديد) ---

        public async Task<List<Notification>> GetRecentByUserAsync(string userId, int count = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetAllByUserAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}

