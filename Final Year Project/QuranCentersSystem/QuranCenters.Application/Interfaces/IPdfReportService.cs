using QuranCenters.Application.DTOs;
using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface IPdfReportService
    {
        Task<MonthlyReportData> GetMonthlyReportData(int studentId, int month, int year);
        Task<StudentReportData> GetStudentReportDataAsync(int studentId);
    }

    public class StudentReportData
    {
        public Student Student { get; set; }
        public List<Attendance> Attendances { get; set; }
        public List<Memorization> Memorizations { get; set; }
    }
}
