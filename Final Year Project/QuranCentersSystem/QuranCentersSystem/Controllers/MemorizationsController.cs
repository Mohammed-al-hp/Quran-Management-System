using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    [Authorize]
    public class MemorizationsController : Controller
    {
        private readonly IMemorizationService _memorizationService;
        private readonly IStudentService _studentService;

        public MemorizationsController(IMemorizationService memorizationService, IStudentService studentService)
        {
            _memorizationService = memorizationService;
            _studentService = studentService;
        }

        // 1. صفحة الفهرس - تحويل تلقائي لصفحة الإنشاء
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        // 2. صفحة تسجيل التسميع اليومي
        public async Task<IActionResult> Create(int? studentId)
        {
            var students = await _studentService.GetAllStudentsAsync();
            var activeStudents = students.Where(s => s.Status == "نشط");
            ViewBag.StudentId = new SelectList(activeStudents, "Id", "Name", studentId);
            ViewBag.RecentMemorizations = await _memorizationService.GetRecentRecordsAsync(10);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Memorization memorization)
        {
            if (ModelState.IsValid)
            {
                await _memorizationService.CreateRecordAsync(memorization);
                TempData["Success"] = "تم إضافة سجل الحفظ بنجاح!";
                return RedirectToAction(nameof(Create), new { studentId = memorization.StudentId });
            }

            var students = await _studentService.GetAllStudentsAsync();
            var activeStudents = students.Where(s => s.Status == "نشط");
            ViewBag.StudentId = new SelectList(activeStudents, "Id", "Name", memorization.StudentId);
            return View(memorization);
        }

        // 3. صفحة المتابعة المتقدمة (التسميع والأسئلة)
        public async Task<IActionResult> FollowUp()
        {
            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.Students = students.Where(s => s.Status == "نشط").ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FollowUp(Memorization memorization, string[] Questions, string[] Answers)
        {
            if (ModelState.IsValid)
            {
                await _memorizationService.CreateRecordWithQuestionsAsync(memorization, Questions, Answers);
                TempData["Success"] = "تم حفظ سجل المتابعة المتقدمة بنجاح!";
                return RedirectToAction(nameof(FollowUp));
            }

            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.Students = students.Where(s => s.Status == "نشط").ToList();
            return View(memorization);
        }

        // 4. صفحة السجل اليومي (عرض سريع للمسجلين اليوم)
        public async Task<IActionResult> DailyRecord()
        {
            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.Students = students.Where(s => s.Status == "نشط").ToList();
            ViewBag.RecentRecords = await _memorizationService.GetTodayRecordsAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DailyRecord(Memorization memorization)
        {
            if (ModelState.IsValid)
            {
                await _memorizationService.CreateRecordAsync(memorization);
                TempData["Success"] = "تم تسجيل السجل بنجاح!";
                return RedirectToAction(nameof(DailyRecord));
            }

            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.Students = students.Where(s => s.Status == "نشط").ToList();
            return View(memorization);
        }
    }
}