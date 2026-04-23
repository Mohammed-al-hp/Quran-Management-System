using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TeachersController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly ICircleService _circleService;

        public TeachersController(ITeacherService teacherService, ICircleService circleService)
        {
            _teacherService = teacherService;
            _circleService = circleService;
        }

        public async Task<IActionResult> Index(string searchTerm, string sortBy)
        {
            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchTerm;

            var teachers = await _teacherService.GetAllTeachersAsync(searchTerm, sortBy);
            return View(teachers);
        }

        public async Task<IActionResult> Create()
        {
            var circles = await _circleService.GetAllCirclesAsync();
            ViewBag.Circles = new SelectList(circles, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher, IFormFile? teacherImage, int? selectedCircleId)
        {
            if (ModelState.IsValid)
            {
                if (teacherImage != null && teacherImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(teacherImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/teachers", fileName);
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/teachers"));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await teacherImage.CopyToAsync(stream);
                    }
                    teacher.ImagePath = "/images/teachers/" + fileName;
                }

                await _teacherService.CreateTeacherAsync(teacher, selectedCircleId);
                return RedirectToAction(nameof(Index));
            }

            var circles = await _circleService.GetAllCirclesAsync();
            ViewBag.Circles = new SelectList(circles, "Id", "Name");
            return View(teacher);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _teacherService.GetTeacherByIdAsync(id.Value);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,Qualification")] Teacher teacher)
        {
            if (id != teacher.Id) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    await _teacherService.UpdateTeacherAsync(teacher);
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _teacherService.GetTeacherWithCirclesAsync(id.Value);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _teacherService.DeleteTeacherAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}