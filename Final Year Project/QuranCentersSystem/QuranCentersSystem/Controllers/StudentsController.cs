using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Rotativa.AspNetCore;

namespace QuranCentersSystem.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? circleId)
        {
            ViewBag.Circles = await _context.Circles.ToListAsync();
            ViewBag.SelectedCircle = circleId;

            var studentsQuery = _context.Students.Include(s => s.Circle).AsQueryable();

            if (circleId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.CircleId == circleId.Value);
            }

            var students = await studentsQuery.ToListAsync();
            return View(students);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .Include(s => s.PointsLedgers)
                .Include(s => s.StudentBadges)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            // Analytics Logic: Pages This Week vs Last Week
            var today = DateTime.Today;
            var startOfThisWeek = today.AddDays(-(int)today.DayOfWeek); // Sunday
            var startOfLastWeek = startOfThisWeek.AddDays(-7);

            var thisWeekProgress = student.Memorizations
                .Where(m => m.Date >= startOfThisWeek && m.Date < startOfThisWeek.AddDays(7))
                .Sum(m => m.PagesCount);

            var lastWeekProgress = student.Memorizations
                .Where(m => m.Date >= startOfLastWeek && m.Date < startOfThisWeek)
                .Sum(m => m.PagesCount);

            ViewBag.ThisWeekPages = thisWeekProgress;
            ViewBag.LastWeekPages = lastWeekProgress;
            
            // Monthly Trend (optional but good for premium feel)
            var monthlyData = student.Memorizations
                .Where(m => m.Date >= today.AddDays(-30))
                .GroupBy(m => m.Date.Date)
                .Select(g => new { Date = g.Key.ToString("MM/dd"), Count = g.Sum(m => m.PagesCount) })
                .ToList();
            
            ViewBag.MonthlyLabels = monthlyData.Select(d => d.Date).ToList();
            ViewBag.MonthlyValues = monthlyData.Select(d => d.Count).ToList();

            return View(student);
        }

        public async Task<IActionResult> PrintStudentReport(int id)
        {
            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return new ViewAsPdf("StudentReportPDF", student)
            {
                FileName = "StudentReport.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--encoding utf-8"
            };
        }

        // 2. ??? ???? ????? ???? ???? (???? ?????? ????? ?????? ??????)
        public IActionResult Create()
        {
            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name");
            // ?? ????? ????? ?????? ??????
            ViewBag.ParentId = new SelectList(_context.Set<Parent>(), "Id", "Name");
            return View();
        }

        // 3. ??????? ?????? ?????? ?????? (???? ???????? ParentId)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,BirthDate,ParentPhoneNumber,AgreedToTerms,CircleId,ParentId")] Student student)
        {
            if (!student.AgreedToTerms)
            {
                ModelState.AddModelError("AgreedToTerms", "??? ???????? ??? ???? ???????? ??????");
            }

            if (ModelState.IsValid)
            {
                student.Status = "???";
                student.JoinDate = DateTime.Now;

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            // ?? ????? ????? ??????? ?? ??? ??? ??? Validation
            ViewBag.ParentId = new SelectList(_context.Set<Parent>(), "Id", "Name", student.ParentId);
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            // ?? ????? ????? ?????? ?????? ??? ???????
            ViewBag.ParentId = new SelectList(_context.Set<Parent>(), "Id", "Name", student.ParentId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,BirthDate,ParentPhoneNumber,AgreedToTerms,CircleId,ParentId,Status,JoinDate")] Student student)
        {
            if (id != student.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(student);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.Id == student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            ViewBag.ParentId = new SelectList(_context.Set<Parent>(), "Id", "Name", student.ParentId);
            return View(student);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.Circle)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}