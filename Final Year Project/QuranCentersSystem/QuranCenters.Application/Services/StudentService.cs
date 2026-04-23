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
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Student>> GetAllStudentsAsync(int? circleId = null)
        {
            var query = _unitOfWork.Repository<Student>().Query()
                .Include(s => s.Circle);

            if (circleId.HasValue)
            {
                query = query.Where(s => s.CircleId == circleId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Student>().GetByIdAsync(id);
        }

        public async Task<Student?> GetStudentWithDetailsAsync(int id)
        {
            return await _unitOfWork.Repository<Student>().Query()
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .Include(s => s.PointsLedgers)
                .Include(s => s.StudentBadges)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Student?> GetStudentByEmailAsync(string email)
        {
            return await _unitOfWork.Repository<Student>().Query()
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .FirstOrDefaultAsync(s => s.ParentEmail == email || s.Phone == email);
        }

        public async Task CreateStudentAsync(Student student)
        {
            student.Status = "نشط";
            student.JoinDate = DateTime.Now;
            await _unitOfWork.Repository<Student>().AddAsync(student);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateStudentAsync(Student student)
        {
            _unitOfWork.Repository<Student>().Update(student);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteStudentAsync(int id)
        {
            var student = await _unitOfWork.Repository<Student>().GetByIdAsync(id);
            if (student != null)
            {
                _unitOfWork.Repository<Student>().Delete(student);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<bool> StudentExistsAsync(int id)
        {
            return await _unitOfWork.Repository<Student>().Query()
                .AnyAsync(s => s.Id == id);
        }

        public async Task<int> GetStudentsCountAsync()
        {
            return await _unitOfWork.Repository<Student>().Query().CountAsync();
        }

        public async Task<StudentWeeklyProgress> GetWeeklyProgressAsync(int studentId)
        {
            var today = DateTime.Today;
            var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek);
            var startOfLastWeek = startOfThisWeek.AddDays(-7);

            var memorizations = await _unitOfWork.Repository<Memorization>().Query()
                .Where(m => m.StudentId == studentId && m.Date >= startOfLastWeek)
                .ToListAsync();

            return new StudentWeeklyProgress
            {
                ThisWeekPages = memorizations
                    .Where(m => m.Date >= startOfThisWeek && m.Date < startOfThisWeek.AddDays(7))
                    .Sum(m => m.PagesCount),
                LastWeekPages = memorizations
                    .Where(m => m.Date >= startOfLastWeek && m.Date < startOfThisWeek)
                    .Sum(m => m.PagesCount)
            };
        }

        public async Task<List<DailyProgressPoint>> GetMonthlyTrendAsync(int studentId)
        {
            var today = DateTime.Today;
            var memorizations = await _unitOfWork.Repository<Memorization>().Query()
                .Where(m => m.StudentId == studentId && m.Date >= today.AddDays(-30))
                .ToListAsync();

            return memorizations
                .GroupBy(m => m.Date.Date)
                .Select(g => new DailyProgressPoint
                {
                    Date = g.Key.ToString("MM/dd"),
                    Count = g.Sum(m => m.PagesCount)
                })
                .OrderBy(d => d.Date)
                .ToList();
        }
    }
}
