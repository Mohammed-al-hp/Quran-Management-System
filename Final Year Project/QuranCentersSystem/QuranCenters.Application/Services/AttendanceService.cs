using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGamificationService _gamificationService;

        public AttendanceService(IUnitOfWork unitOfWork, IGamificationService gamificationService)
        {
            _unitOfWork = unitOfWork;
            _gamificationService = gamificationService;
        }

        public async Task<IEnumerable<Attendance>> GetAllAsync()
        {
            return await _unitOfWork.Repository<Attendance>().Query()
                .Include(a => a.Student)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByStudentAsync(int studentId)
        {
            return await _unitOfWork.Repository<Attendance>().Query()
                .Where(a => a.StudentId == studentId)
                .OrderByDescending(a => a.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByDateAsync(DateTime date)
        {
            return await _unitOfWork.Repository<Attendance>().Query()
                .Include(a => a.Student)
                .Where(a => a.Date.Date == date.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Attendance>> GetByCircleAndDateAsync(int circleId, DateTime date)
        {
            return await _unitOfWork.Repository<Attendance>().Query()
                .Include(a => a.Student)
                .Where(a => a.Student.CircleId == circleId && a.Date.Date == date.Date)
                .ToListAsync();
        }

        public async Task RecordAttendanceAsync(Attendance attendance)
        {
            await _unitOfWork.Repository<Attendance>().AddAsync(attendance);
            await _unitOfWork.SaveChangesAsync();

            await _gamificationService.ProcessAttendancePointsAsync(attendance);
        }

        public async Task RecordBulkAttendanceAsync(IEnumerable<Attendance> attendances)
        {
            await _unitOfWork.Repository<Attendance>().AddRangeAsync(attendances);
            await _unitOfWork.SaveChangesAsync();

            foreach (var att in attendances)
            {
                await _gamificationService.ProcessAttendancePointsAsync(att);
            }
        }

        public async Task<int> GetTodayAttendanceCountAsync(List<int>? circleIds = null)
        {
            var query = _unitOfWork.Repository<Attendance>().Query()
                .Where(a => a.Date.Date == DateTime.Today);

            if (circleIds != null && circleIds.Count > 0)
            {
                query = query.Where(a => circleIds.Contains(a.Student.CircleId));
            }

            return await query.CountAsync();
        }

        public async Task SaveGroupAttendanceAsync(int circleId, DateTime date, Dictionary<int, string> statuses, Dictionary<int, string> notes)
        {
            if (statuses == null || statuses.Count == 0) return;

            foreach (var item in statuses)
            {
                int studentId = item.Key;
                string status = item.Value;
                string note = notes != null && notes.ContainsKey(studentId) ? notes[studentId] : "";

                // التحقق من وجود سجل حضور سابق لنفس الطالب في نفس اليوم
                var existingAttendance = await _unitOfWork.Repository<Attendance>().Query()
                    .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == date.Date);

                if (existingAttendance != null)
                {
                    existingAttendance.Status = status;
                    existingAttendance.Notes = note;
                    _unitOfWork.Repository<Attendance>().Update(existingAttendance);
                }
                else
                {
                    var attendance = new Attendance
                    {
                        Date = date,
                        Status = status,
                        StudentId = studentId,
                        Notes = note
                    };
                    await _unitOfWork.Repository<Attendance>().AddAsync(attendance);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            // Process gamification points for present students
            foreach (var item in statuses.Where(s => s.Value == "حاضر"))
            {
                var attendance = new Attendance { StudentId = item.Key, Status = item.Value };
                await _gamificationService.ProcessAttendancePointsAsync(attendance);
            }
        }
    }
}

