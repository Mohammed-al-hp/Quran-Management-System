using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Microsoft.AspNetCore.Authorization; // 👈 سطر استدعاء مكتبة الصلاحيات

namespace QuranCentersSystem.Controllers
{
    [Authorize] // 👈 قفل الشاشات وجعلها تتطلب تسجيل الدخول
    public class MemorizationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MemorizationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. شاشة الـ Index (التحويل التلقائي)
        // ==========================================
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        // ==========================================
        // 2. شاشة التسجيل اليومي الافتراضية
        // ==========================================
        public IActionResult Create(int? studentId)
        {
            ViewBag.StudentId = new SelectList(_context.Students.Where(s => s.Status == "نشط"), "Id", "Name", studentId);

            ViewBag.RecentMemorizations = _context.Memorizations
                .Include(m => m.Student)
                .OrderByDescending(m => m.Date)
                .Take(10)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Memorization memorization)
        {
            if (ModelState.IsValid)
            {
                _context.Memorizations.Add(memorization);
                _context.SaveChanges();

                TempData["Success"] = "تم تسجيل درجة الحفظ بنجاح!";
                return RedirectToAction(nameof(Create), new { studentId = memorization.StudentId });
            }

            ViewBag.StudentId = new SelectList(_context.Students.Where(s => s.Status == "نشط"), "Id", "Name", memorization.StudentId);
            return View(memorization);
        }

        // ==========================================
        // 3. شاشة المتابعة والأسئلة (نظام الأسئلة)
        // ==========================================
        public async Task<IActionResult> FollowUp()
        {
            ViewBag.Students = await _context.Students.Where(s => s.Status == "نشط").ToListAsync();
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

                TempData["Success"] = "تم حفظ جلسة المتابعة والأسئلة بنجاح!";
                return RedirectToAction(nameof(FollowUp));
            }

            ViewBag.Students = await _context.Students.Where(s => s.Status == "نشط").ToListAsync();
            return View(memorization);
        }

        // ==========================================
        // 4. شاشة التتبيع المباشر (تسميع بدون أسئلة)
        // ==========================================
        public async Task<IActionResult> DailyRecord()
        {
            ViewBag.Students = await _context.Students.Where(s => s.Status == "نشط").ToListAsync();

            // جلب آخر 5 عمليات حفظ تمت اليوم لقرائتها في يسار الشاشة
            var today = DateTime.Today;
            var recentRecords = await _context.Memorizations
                .Include(m => m.Student)
                .Where(m => m.Date >= today)
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();

            // إرسال العمليات الأخيرة عبر الـ ViewBag
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

                TempData["Success"] = "تم تسجيل التتبيع للحفظ بنجاح!";
                return RedirectToAction(nameof(DailyRecord));
            }

            ViewBag.Students = await _context.Students.Where(s => s.Status == "نشط").ToListAsync();
            return View(memorization);
        }
    }
}