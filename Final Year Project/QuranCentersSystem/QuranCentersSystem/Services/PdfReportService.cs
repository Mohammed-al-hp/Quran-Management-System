using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Services
{
    /// <summary>
    /// خدمة تقارير PDF - توليد تقارير شهرية احترافية للطلاب
    /// تستخدم Rotativa لتحويل صفحات HTML إلى PDF
    /// </summary>
    public class PdfReportService
    {
        private readonly ApplicationDbContext _context;

        public PdfReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// جمع بيانات التقرير الشهري لطالب معين
        /// </summary>
        /// <param name="studentId">معرف الطالب</param>
        /// <param name="month">الشهر (1-12)</param>
        /// <param name="year">السنة</param>
        /// <returns>بيانات التقرير الشهري</returns>
        public async Task<MonthlyReportData> GetMonthlyReportData(int studentId, int month, int year)
        {
            var student = await _context.Students
                .Include(s => s.Circle)
                    .ThenInclude(c => c.Teacher)
                .FirstOrDefaultAsync(s => s.Id == studentId);

            if (student == null) return null;

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // بيانات الحضور للشهر
            var attendances = await _context.Attendances
                .Where(a => a.StudentId == studentId && a.Date >= startDate && a.Date <= endDate)
                .OrderBy(a => a.Date)
                .ToListAsync();

            // بيانات الحفظ للشهر
            var memorizations = await _context.Memorizations
                .Where(m => m.StudentId == studentId && m.Date >= startDate && m.Date <= endDate)
                .OrderBy(m => m.Date)
                .ToListAsync();

            // حساب الإحصائيات
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

                // الحضور
                TotalDays = totalDays,
                PresentDays = presentDays,
                AbsentDays = absentDays,
                AttendanceRate = totalDays > 0 ? ((double)presentDays / totalDays * 100).ToString("F1") + "%" : "N/A",

                // الحفظ
                TotalSessions = totalSessions,
                ExcellentCount = excellentCount,
                VeryGoodCount = veryGoodCount,
                GoodCount = goodCount,
                FairCount = fairCount,

                // التفاصيل
                Attendances = attendances,
                Memorizations = memorizations,

                // الملخص
                OverallAssessment = GetOverallAssessment(presentDays, totalDays, excellentCount, totalSessions)
            };
        }

        /// <summary>
        /// تقييم شامل بناءً على الحضور والدرجات
        /// </summary>
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

    /// <summary>
    /// نموذج بيانات التقرير الشهري
    /// </summary>
    public class MonthlyReportData
    {
        public string StudentName { get; set; }
        public string CircleName { get; set; }
        public string TeacherName { get; set; }
        public string Month { get; set; }
        public string ReportDate { get; set; }

        // الحضور
        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public string AttendanceRate { get; set; }

        // الحفظ
        public int TotalSessions { get; set; }
        public int ExcellentCount { get; set; }
        public int VeryGoodCount { get; set; }
        public int GoodCount { get; set; }
        public int FairCount { get; set; }

        // التفاصيل
        public List<Attendance> Attendances { get; set; }
        public List<Memorization> Memorizations { get; set; }

        // الملخص
        public string OverallAssessment { get; set; }
    }
}
