using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface INotificationService
    {
        // --- الإشعارات التلقائية (الموجودة سابقاً) ---
        Task NotifyParentOnAttendance(int studentId);
        Task NotifyStudentOnNewTask(int studentId, string surahName);
        Task NotifyOnNewGrade(int studentId, string grade, string surahName);
        Task NotifyParentOnNewBadge(int studentId, string badgeName);
        Task NotifyParentOnPointsAwarded(int studentId, int points, string reason);
        Task SendBulkNotification(string[] userIds, string title, string message, string type);

        // --- إدارة الإشعارات (جديد - للمتحكمات) ---
        Task<List<Notification>> GetRecentByUserAsync(string userId, int count = 10);
        Task<List<Notification>> GetAllByUserAsync(string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
        Task MarkAllAsReadAsync(string userId);
    }
}
