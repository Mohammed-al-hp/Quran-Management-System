using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using Rotativa.AspNetCore;

namespace QuranCentersSystem.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly ICircleService _circleService;
        private readonly IUnitOfWork _unitOfWork;

        public StudentsController(IStudentService studentService, ICircleService circleService, IUnitOfWork unitOfWork)
        {
            _studentService = studentService;
            _circleService = circleService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index(int? circleId)
        {
            ViewBag.Circles = await _circleService.GetAllCirclesAsync();
            ViewBag.SelectedCircle = circleId;

            var students = await _studentService.GetAllStudentsAsync(circleId);
            return View(students);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _studentService.GetStudentWithDetailsAsync(id.Value);
            if (student == null) return NotFound();

            // Analytics
            var weeklyProgress = await _studentService.GetWeeklyProgressAsync(id.Value);
            ViewBag.ThisWeekPages = weeklyProgress.ThisWeekPages;
            ViewBag.LastWeekPages = weeklyProgress.LastWeekPages;

            var monthlyData = await _studentService.GetMonthlyTrendAsync(id.Value);
            ViewBag.MonthlyLabels = monthlyData.Select(d => d.Date).ToList();
            ViewBag.MonthlyValues = monthlyData.Select(d => d.Count).ToList();

            return View(student);
        }

        public async Task<IActionResult> PrintStudentReport(int id)
        {
            var student = await _studentService.GetStudentWithDetailsAsync(id);
            if (student == null) return NotFound();

            return new ViewAsPdf("StudentReportPDF", student)
            {
                FileName = "StudentReport.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--encoding utf-8"
            };
        }

        public async Task<IActionResult> Create()
        {
            var circles = await _circleService.GetAllCirclesAsync();
            ViewData["CircleId"] = new SelectList(circles, "Id", "Name");
            var parents = await _unitOfWork.Repository<Parent>().GetAllAsync();
            ViewBag.ParentId = new SelectList(parents, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,BirthDate,ParentPhoneNumber,AgreedToTerms,CircleId,ParentId")] Student student)
        {
            if (!student.AgreedToTerms)
            {
                ModelState.AddModelError("AgreedToTerms", "يجب الموافقة على شروط وأحكام النظام");
            }

            if (ModelState.IsValid)
            {
                await _studentService.CreateStudentAsync(student);
                return RedirectToAction(nameof(Index));
            }

            var circles = await _circleService.GetAllCirclesAsync();
            ViewData["CircleId"] = new SelectList(circles, "Id", "Name", student.CircleId);
            var parents = await _unitOfWork.Repository<Parent>().GetAllAsync();
            ViewBag.ParentId = new SelectList(parents, "Id", "Name", student.ParentId);
            return View(student);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _studentService.GetStudentByIdAsync(id.Value);
            if (student == null) return NotFound();

            var circles = await _circleService.GetAllCirclesAsync();
            ViewData["CircleId"] = new SelectList(circles, "Id", "Name", student.CircleId);
            var parents = await _unitOfWork.Repository<Parent>().GetAllAsync();
            ViewBag.ParentId = new SelectList(parents, "Id", "Name", student.ParentId);
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
                    await _studentService.UpdateStudentAsync(student);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await _studentService.StudentExistsAsync(student.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var circles = await _circleService.GetAllCirclesAsync();
            ViewData["CircleId"] = new SelectList(circles, "Id", "Name", student.CircleId);
            var parents = await _unitOfWork.Repository<Parent>().GetAllAsync();
            ViewBag.ParentId = new SelectList(parents, "Id", "Name", student.ParentId);
            return View(student);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var student = await _studentService.GetStudentWithDetailsAsync(id.Value);
            if (student == null) return NotFound();

            return View(student);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _studentService.DeleteStudentAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}