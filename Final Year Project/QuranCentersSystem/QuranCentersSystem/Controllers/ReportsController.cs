using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using QuranCenters.Application.Interfaces;
using System;
using System.Threading.Tasks;
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
        private readonly IPdfReportService _pdfReportService;

        public ReportsController(IPdfReportService pdfReportService)
        {
            _pdfReportService = pdfReportService;
        }

        /// <summary>
        /// عرض تقرير طالب على الشاشة
        /// </summary>
        public async Task<IActionResult> StudentReport(int id)
        {
            var reportData = await _pdfReportService.GetStudentReportDataAsync(id);
            if (reportData == null) return NotFound();

            ViewBag.Student = reportData.Student;
            ViewBag.Attendances = reportData.Attendances;
            ViewBag.Memorization = reportData.Memorizations;

            return View();
        }

        /// <summary>
        /// طباعة تقرير طالب كملف PDF
        /// </summary>
        public async Task<IActionResult> PrintStudentReport(int id)
        {
            var reportData = await _pdfReportService.GetStudentReportDataAsync(id);
            if (reportData == null) return NotFound();

            ViewBag.Student = reportData.Student;
            ViewBag.Attendances = reportData.Attendances;
            ViewBag.Memorization = reportData.Memorizations;

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
        public async Task<IActionResult> MonthlyReport(int studentId, int? month, int? year)
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
        public async Task<IActionResult> PrintMonthlyReport(int studentId, int? month, int? year)
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