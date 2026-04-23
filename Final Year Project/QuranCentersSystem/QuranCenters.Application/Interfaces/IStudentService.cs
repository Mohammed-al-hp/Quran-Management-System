using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetAllStudentsAsync(int? circleId = null);
        Task<Student?> GetStudentByIdAsync(int id);
        Task<Student?> GetStudentWithDetailsAsync(int id);
        Task<Student?> GetStudentByEmailAsync(string email);
        Task CreateStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
        Task DeleteStudentAsync(int id);
        Task<bool> StudentExistsAsync(int id);
        Task<int> GetStudentsCountAsync();
        Task<StudentWeeklyProgress> GetWeeklyProgressAsync(int studentId);
        Task<List<DailyProgressPoint>> GetMonthlyTrendAsync(int studentId);
    }

    public class StudentWeeklyProgress
    {
        public decimal ThisWeekPages { get; set; }
        public decimal LastWeekPages { get; set; }
    }

    public class DailyProgressPoint
    {
        public string Date { get; set; }
        public decimal Count { get; set; }
    }
}
