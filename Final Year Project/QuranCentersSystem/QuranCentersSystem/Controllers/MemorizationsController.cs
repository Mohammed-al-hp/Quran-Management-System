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
using Microsoft.AspNetCore.Authorization; // ?? ??? ??????? ????? ?????????

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")] // ???? ?????? ??????? ???
    [Authorize] // ?? ??? ??????? ?????? ????? ????? ??????
    public class MemorizationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly QuranCenters.Application.Interfaces.IGamificationService _gamificationService;

        public MemorizationsController(ApplicationDbContext context, QuranCenters.Application.Interfaces.IGamificationService gamificationService)
        {
            _context = context;
            _gamificationService = gamificationService;
        }

        // ==========================================
        // 1. ???? ??? Index (??????? ????????)
        // ==========================================
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        // ==========================================
        // 2. ???? ??????? ?????? ??????????
        // ==========================================
        public IActionResult Create(int? studentId)
        {
            ViewBag.StudentId = new SelectList(_context.Students.Where(s => s.Status == "???"), "Id", "Name", studentId);

            ViewBag.RecentMemorizations = _context.Memorizations
                .Include(m => m.Student)
                .OrderByDescending(m => m.Date)
                .Take(10)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Memorization memorization)
        {
            if (ModelState.IsValid)
            {
                _context.Memorizations.Add(memorization);
                await _context.SaveChangesAsync();

                await _gamificationService.ProcessDailyMemorizationPointsAsync(memorization);

                TempData["Success"] = "تم إضافة سجل الحفظ بنجاح!";
                return RedirectToAction(nameof(Create), new { studentId = memorization.StudentId });
            }

            ViewBag.StudentId = new SelectList(_context.Students.Where(s => s.Status == "???"), "Id", "Name", memorization.StudentId);
            return View(memorization);
        }

        // ==========================================
        // 3. ???? ???????? ???????? (???? ???????)
        // ==========================================
        public async Task<IActionResult> FollowUp()
        {
            ViewBag.Students = await _context.Students.Where(s => s.Status == "???").ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FollowUp(Memorization memorization, string[] Questions, string[] Answers)
        {
            if (ModelState.IsValid)
            {
                _context.Add(memorization);
                await _context.SaveChangesAsync();

                if (Questions != null)
                {
                    for (int i = 0; i < Questions.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(Questions[i]))
                        {
                            var q = new MemorizationQuestion
                            {
                                MemorizationId = memorization.Id,
                                QuestionText = Questions[i],
                                StudentAnswer = Answers.Length > i ? Answers[i] : ""
                            };
                            _context.MemorizationQuestions.Add(q);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "?? ??? ???? ???????? ???????? ?????!";
                return RedirectToAction(nameof(FollowUp));
            }

            ViewBag.Students = await _context.Students.Where(s => s.Status == "???").ToListAsync();
            return View(memorization);
        }

        // ==========================================
        // 4. ???? ??????? ??????? (????? ???? ?????)
        // ==========================================
        public async Task<IActionResult> DailyRecord()
        {
            ViewBag.Students = await _context.Students.Where(s => s.Status == "???").ToListAsync();

            // ??? ??? 5 ?????? ??? ??? ????? ???????? ?? ???? ??????
            var today = DateTime.Today;
            var recentRecords = await _context.Memorizations
                .Include(m => m.Student)
                .Where(m => m.Date >= today)
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();

            // ????? ???????? ??????? ??? ??? ViewBag
            ViewBag.RecentRecords = recentRecords;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DailyRecord(Memorization memorization)
        {
            if (ModelState.IsValid)
            {
                _context.Add(memorization);
                await _context.SaveChangesAsync();

                await _gamificationService.ProcessDailyMemorizationPointsAsync(memorization);

                TempData["Success"] = "تم تسجيل السجل بنجاح!";
                return RedirectToAction(nameof(DailyRecord));
            }

            ViewBag.Students = await _context.Students.Where(s => s.Status == "???").ToListAsync();
            return View(memorization);
        }
    }
}