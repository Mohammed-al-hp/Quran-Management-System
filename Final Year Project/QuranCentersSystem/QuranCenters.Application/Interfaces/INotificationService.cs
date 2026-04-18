using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface INotificationService
    {
        Task NotifyParentOnAttendance(int studentId);
        Task NotifyStudentOnNewTask(int studentId, string surahName);
        Task NotifyOnNewGrade(int studentId, string grade, string surahName);
        Task NotifyParentOnNewBadge(int studentId, string badgeName);
        Task NotifyParentOnPointsAwarded(int studentId, int points, string reason);
        Task SendBulkNotification(string[] userIds, string title, string message, string type);
    }
}
