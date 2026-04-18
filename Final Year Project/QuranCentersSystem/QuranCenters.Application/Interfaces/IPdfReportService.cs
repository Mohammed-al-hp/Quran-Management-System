using QuranCenters.Application.DTOs;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface IPdfReportService
    {
        Task<MonthlyReportData> GetMonthlyReportData(int studentId, int month, int year);
    }
}
