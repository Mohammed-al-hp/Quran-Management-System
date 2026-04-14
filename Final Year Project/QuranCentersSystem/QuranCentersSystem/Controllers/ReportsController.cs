using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuranCentersSystem.Data;
using QuranCentersSystem.Services;
using System;
using System.Linq;
using Rotativa.AspNetCore;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// متحكم التقارير - يولد تقارير الطلاب بصيغة PDF (يومية وشهرية)
    /// محمي بالمصادقة - يتطلب تسجيل الدخول
    /// </summary>
    [Authorize]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfReportService _pdfReportService;

        public ReportsController(ApplicationDbContext context, PdfReportService pdfReportService)
        {
            _context = context;
            _pdfReportService = pdfReportService;
        }

        /// <summary>
        /// عرض تقرير طالب على الشاشة
        /// </summary>
        public IActionResult StudentReport(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);

            var attendances = _context.Attendances
                .Where(a => a.StudentId == id)
                .ToList();

            var memorization = _context.Memorizations
                .Where(m => m.StudentId == id)
                .ToList();

            ViewBag.Student = student;
            ViewBag.Attendances = attendances;
            ViewBag.Memorization = memorization;

            return View();
        }

        /// <summary>
        /// طباعة تقرير طالب كملف PDF
        /// </summary>
        public IActionResult PrintStudentReport(int id)
        {
            var student = _context.Students.FirstOrDefault(s => s.Id == id);

            var attendances = _context.Attendances
                .Where(a => a.StudentId == id)
                .ToList();

            var memorization = _context.Memorizations
                .Where(m => m.StudentId == id)
                .ToList();

            ViewBag.Student = student;
            ViewBag.Attendances = attendances;
            ViewBag.Memorization = memorization;

            return new ViewAsPdf("StudentReport")
            {
                FileName = "StudentReport.pdf",
                CustomSwitches = "--encoding utf-8"
            };
        }

        /// <summary>
        /// توليد تقرير شهري احترافي لطالب معين
        /// GET: /Reports/MonthlyReport?studentId=1&month=4&year=2026
        /// </summary>
        public async System.Threading.Tasks.Task<IActionResult> MonthlyReport(int studentId, int? month, int? year)
        {
            var reportMonth = month ?? DateTime.Now.Month;
            var reportYear = year ?? DateTime.Now.Year;

            var reportData = await _pdfReportService.GetMonthlyReportData(studentId, reportMonth, reportYear);

            if (reportData == null)
                return NotFound("الطالب غير موجود");

            return View("MonthlyStudentReport", reportData);
        }

        /// <summary>
        /// طباعة التقرير الشهري كملف PDF
        /// GET: /Reports/PrintMonthlyReport?studentId=1&month=4&year=2026
        /// </summary>
        public async System.Threading.Tasks.Task<IActionResult> PrintMonthlyReport(int studentId, int? month, int? year)
        {
            var reportMonth = month ?? DateTime.Now.Month;
            var reportYear = year ?? DateTime.Now.Year;

            var reportData = await _pdfReportService.GetMonthlyReportData(studentId, reportMonth, reportYear);

            if (reportData == null)
                return NotFound("الطالب غير موجود");

            return new ViewAsPdf("MonthlyStudentReport", reportData)
            {
                FileName = $"MonthlyReport_{reportData.StudentName}_{reportMonth}_{reportYear}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--encoding utf-8"
            };
        }
    }
}