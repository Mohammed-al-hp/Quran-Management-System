using QuranCenters.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAllAsync();
        Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId);
        Task<IEnumerable<Attendance>> GetByDateAsync(DateTime date);
        Task<IEnumerable<Attendance>> GetByCircleAndDateAsync(int circleId, DateTime date);
        Task RecordAttendanceAsync(Attendance attendance);
        Task RecordBulkAttendanceAsync(IEnumerable<Attendance> attendances);
        Task<int> GetTodayAttendanceCountAsync(List<int>? circleIds = null);
    }
}
