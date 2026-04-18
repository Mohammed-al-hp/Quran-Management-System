using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")] // ???? ?????? ??????? ???
    public class AttendancesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AttendancesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. ??? ???? ?????? (???????)
        public async Task<IActionResult> Index()
        {
            var attendances = _context.Attendances.Include(a => a.Student);
            return View(await attendances.ToListAsync());
        }

        // 2. ???? ??????? ??????? (GET)
        public IActionResult Create()
        {
            ViewBag.StudentId = new SelectList(_context.Students, "Id", "Name");
            return View();
        }

        // 3. ??? ??????? ??????? (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Status,Notes,StudentId")] Attendance attendance)
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

        // =========================================================
        // ???? ??????? ??????? ?????? ???? ??????? ???????
        // =========================================================

        // 4. ???? ??? ???? ???? ????? ?????? ?????? (GET)
        public async Task<IActionResult> GroupAttendance(int? circleId)
        {
            // ??? ????? ??????? ?????? ?? ??????? ????????
            ViewBag.Circles = new SelectList(_context.Circles, "Id", "Name", circleId);

            var students = new List<Student>();

            // ??? ????? ???????? ???? ?????? ???? ??????
            if (circleId.HasValue)
            {
                students = await _context.Students
                    .Where(s => s.CircleId == circleId.Value)
                    .ToListAsync();
            }

            ViewBag.SelectedCircleId = circleId;
            return View(students);
        }

        // 5. ??? ?????? ??????? ???? ????? (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroupAttendance(int circleId, DateTime date, Dictionary<int, string> attendanceStatus, Dictionary<int, string> notes)
        {
            if (attendanceStatus != null)
            {
                foreach (var item in attendanceStatus)
                {
                    int studentId = item.Key;
                    string status = item.Value;
                    string note = notes.ContainsKey(studentId) ? notes[studentId] : "";

                    // ?????? ??? ??? ?????? ?? ??? ???? ??? ?????? ?? ??? ????? ?????? ???? ???????
                    var existingAttendance = await _context.Attendances
                        .FirstOrDefaultAsync(a => a.StudentId == studentId && a.Date.Date == date.Date);

                    if (existingAttendance != null)
                    {
                        // ??? ??? ???????? ????? ?????? ?????? ?????????
                        existingAttendance.Status = status;
                        existingAttendance.Notes = note;
                    }
                    else
                    {
                        // ??? ?? ??? ???????? ???? ????? ??????
                        var attendance = new Attendance
                        {
                            Date = date,
                            Status = status,
                            StudentId = studentId,
                            Notes = note
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