using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Microsoft.AspNetCore.Mvc.Localization;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var attendances = _context.Attendances.Include(a => a.Student).OrderByDescending(a => a.Date);
            return View(await attendances.ToListAsync());
        }

        public IActionResult Create()
        {
            ViewBag.StudentId = new SelectList(_context.Students, "Id", "Name");
            return View(new Attendance { Date = DateTime.Now });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Status,Notes,StudentId,DelayMinutes")] Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                _context.Add(attendance);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StudentId = new SelectList(_context.Students, "Id", "Name", attendance.StudentId);
            return View(attendance);
        }

        public async Task<IActionResult> GroupAttendance(int? circleId, DateTime? date)
        {
            ViewBag.Circles = new SelectList(_context.Circles, "Id", "Name", circleId);
            var selectedDate = date ?? DateTime.Now.Date;
            ViewBag.SelectedDate = selectedDate.ToString("yyyy-MM-dd");

            var students = new List<Student>();
            if (circleId.HasValue)
            {
                students = await _context.Students
                    .Where(s => s.CircleId == circleId.Value)
                    .ToListAsync();
            }

            ViewBag.SelectedCircleId = circleId;
            return View(students);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroupAttendance(int circleId, DateTime date, Dictionary<int, string> attendanceStatus, Dictionary<int, string> notes, Dictionary<int, int> delayMinutes)
        {
            if (attendanceStatus != null)
            {
                foreach (var item in attendanceStatus)
                {
                    int studentId = item.Key;
                    string status = item.Value;
                    string note = notes.ContainsKey(studentId) ? notes[studentId] : "";
                    int delay = delayMinutes.ContainsKey(studentId) ? delayMinutes[studentId] : 0;

                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == date.Date);

                    if (existingAttendance != null)
                    {
                        existingAttendance.Status = status;
                        existingAttendance.Notes = note;
                        existingAttendance.DelayMinutes = (status == "متأخر") ? delay : 0;
                    }
                    else
                    {
                        var attendance = new Attendance
                        {
                            Date = date,
                            Status = status,
                            StudentId = studentId,
                            Notes = note,
                            DelayMinutes = (status == "متأخر") ? delay : 0
                        };
                        _context.Attendances.Add(attendance);
                    }
                }
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return RedirectToAction(nameof(GroupAttendance), new { circleId = circleId });
        }
    }
}