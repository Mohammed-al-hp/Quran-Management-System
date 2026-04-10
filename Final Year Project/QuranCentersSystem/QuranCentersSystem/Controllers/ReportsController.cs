using Microsoft.AspNetCore.Mvc;
using QuranCentersSystem.Data;
using System.Linq;
using Rotativa.AspNetCore;

namespace QuranCentersSystem.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

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
                FileName = "StudentReport.pdf"
            };
        }
    }
}