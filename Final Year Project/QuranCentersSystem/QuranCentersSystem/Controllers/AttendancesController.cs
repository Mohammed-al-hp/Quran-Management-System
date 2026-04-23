using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin,Teacher")]
    public class AttendancesController : Controller
    {
        private readonly IAttendanceService _attendanceService;
        private readonly ICircleService _circleService;
        private readonly IStudentService _studentService;

        public AttendancesController(
            IAttendanceService attendanceService,
            ICircleService circleService,
            IStudentService studentService)
        {
            _attendanceService = attendanceService;
            _circleService = circleService;
            _studentService = studentService;
        }

        // 1. عرض سجل الحضور (الفهرس)
        public async Task<IActionResult> Index()
        {
            var attendances = await _attendanceService.GetAllAsync();
            return View(attendances);
        }

        // 2. صفحة التسجيل الفردي (GET)
        public async Task<IActionResult> Create()
        {
            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.StudentId = new SelectList(students, "Id", "Name");
            return View();
        }

        // 3. حفظ التسجيل الفردي (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Date,Status,Notes,StudentId")] Attendance attendance)
        {
            if (ModelState.IsValid)
            {
                await _attendanceService.RecordAttendanceAsync(attendance);
                return RedirectToAction(nameof(Index));
            }
            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.StudentId = new SelectList(students, "Id", "Name", attendance.StudentId);
            return View(attendance);
        }

        // =========================================================
        // نظام التسجيل الجماعي حسب الحلقة والتاريخ
        // =========================================================

        // 4. صفحة حضور جماعي - عرض طلاب حلقة معينة (GET)
        public async Task<IActionResult> GroupAttendance(int? circleId)
        {
            var circles = await _circleService.GetAllCirclesAsync();
            ViewBag.Circles = new SelectList(circles, "Id", "Name", circleId);

            var students = new List<Student>();

            if (circleId.HasValue)
            {
                students = new List<Student>(await _studentService.GetAllStudentsAsync(circleId));
            }

            ViewBag.SelectedCircleId = circleId;
            return View(students);
        }

        // 5. حفظ الحضور الجماعي دفعة واحدة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGroupAttendance(int circleId, DateTime date, Dictionary<int, string> attendanceStatus, Dictionary<int, string> notes)
        {
            if (attendanceStatus != null && attendanceStatus.Count > 0)
            {
                await _attendanceService.SaveGroupAttendanceAsync(circleId, date, attendanceStatus, notes);
                return RedirectToAction(nameof(Index));
            }

            return RedirectToAction(nameof(GroupAttendance), new { circleId });
        }
    }
}