using QuranCenters.Core.Entities;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface IGamificationService
    {
        Task AwardPointsAsync(int studentId, int points, string reason);
        Task ProcessDailyMemorizationPointsAsync(Memorization memorization);
        Task ProcessAttendancePointsAsync(Attendance attendance);
        Task CheckAndAwardBadgesAsync(int studentId);
    }
}
