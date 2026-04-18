using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using QuranCenters.Application.Interfaces;
using QuranCenters.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Infrastructure.Services
{
    public class PdfReportService : IPdfReportService
    {
        private readonly ApplicationDbContext _context;

        public PdfReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<MonthlyReportData> GetMonthlyReportData(int studentId, int month, int year)
        {
            var student = await _context.Students
                .Include(s => s.Circle)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId && a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ToListAsync();

            var memorizations = await _context.Memorizations
                .Where(m => m.StudentId == studentId && m.Date >= startDate && m.Date <= endDate)
                .OrderBy(m => m.Date)
                .ToListAsync();

            var totalDays = attendances.Count;
            var presentDays = attendances.Count(a => a.Status == "حاضر");
            var absentDays = attendances.Count(a => a.Status == "غائب");

            var totalSessions = memorizations.Count;
            var excellentCount = memorizations.Count(m => m.Grade == "ممتاز");
            var veryGoodCount = memorizations.Count(m => m.Grade == "جيد جداً");
            var goodCount = memorizations.Count(m => m.Grade == "جيد");
            var fairCount = memorizations.Count(m => m.Grade == "مقبول");

            return new MonthlyReportData
            {
                StudentName = student.Name,
                CircleName = student.Circle?.Name ?? "غير محدد",
                TeacherName = student.Circle?.Teacher?.Name ?? "غير محدد",
                Month = startDate.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ar-SA")),
                ReportDate = DateTime.Now.ToString("yyyy/MM/dd"),

                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                AttendanceRate = totalDays > 0 ? ((double)presentDays / totalDays * 100).ToString("F1") + "%" : "N/A",

                TotalSessions = totalSessions,
                ExcellentCount = excellentCount,
                VeryGoodCount = veryGoodCount,
                GoodCount = goodCount,
                FairCount = fairCount,

                Attendances = attendances,
                Memorizations = memorizations,

                OverallAssessment = GetOverallAssessment(presentDays, totalDays, excellentCount, totalSessions)
            };
        }

        private string GetOverallAssessment(int present, int total, int excellent, int sessions)
        {
            if (total == 0 || sessions == 0) return "لا توجد بيانات كافية للتقييم";

            double attendanceRate = (double)present / total;
            double excellenceRate = (double)excellent / sessions;

            if (attendanceRate >= 0.9 && excellenceRate >= 0.7)
                return "أداء متميز - يُنصح بتكثيف الحفظ الجديد";
            if (attendanceRate >= 0.8 && excellenceRate >= 0.5)
                return "أداء جيد جداً - يحتاج لمزيد من المراجعة";
            if (attendanceRate >= 0.7)
                return "أداء جيد - يحتاج لتحسين الانتظام";
            if (attendanceRate >= 0.5)
                return "أداء مقبول - يحتاج للمتابعة والتشجيع";
            return "يحتاج لاهتمام خاص ومتابعة مكثفة";
        }
    }
}
