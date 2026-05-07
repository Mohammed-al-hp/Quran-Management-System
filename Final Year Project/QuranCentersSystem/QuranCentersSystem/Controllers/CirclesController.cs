using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Controllers
{
    public class CirclesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CirclesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض قائمة الحلقات مع الفلترة
        public async Task<IActionResult> Index(string gender, string circleType)
        {
            var query = _context.Circles
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .AsQueryable();

            // فلترة الجنس - القيم المتوقعة: "ذكور" أو "إناث"
            if (!string.IsNullOrEmpty(gender) && gender != "الكل")
            {
                query = query.Where(c => c.Gender == gender);
            }

            // فلترة نوع الحلقة - القيم المتوقعة: "حلقة عامة" أو "دورة صيفية"
            if (!string.IsNullOrEmpty(circleType) && circleType != "الكل")
            {
                query = query.Where(c => c.CircleType == circleType);
            }

            return View(await query.ToListAsync());
        }

        // 2. عرض صفحة إضافة حلقة جديدة
        public IActionResult Create()
        {
            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name");
            return View();
        }

        // 3. معالجة حفظ الحلقة الجديدة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,CircleType,Gender,TeacherId,TimingType,StartPrayer,EndPrayer,StartTime,EndTime")]
            Circle circle,
            string[] SelectedDays,
            int[] TeacherIds)
        {
            ModelState.Remove("Teacher");
            ModelState.Remove("Students");
            ModelState.Remove("SelectedDays");

            if (ModelState.IsValid)
            {
                try
                {
                    // الأيام المختارة
                    circle.SelectedDays = (SelectedDays != null && SelectedDays.Length > 0)
                        ? string.Join(", ", SelectedDays)
                        : "لم يتم تحديد أيام";

                    // أول محفظ مختار يُخزَّن في TeacherId (للتوافق مع الـ Model الحالي)
                    if (TeacherIds != null && TeacherIds.Length > 0)
                    {
                        circle.TeacherId = TeacherIds[0];
                    }

                    _context.Add(circle);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "حدث خطأ أثناء الحفظ: " + ex.Message);
                }
            }

            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name", circle.TeacherId);
            return View(circle);
        }

        // 4. حذف الحلقة
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var circle = await _context.Circles.FindAsync(id);
            if (circle != null)
            {
                _context.Circles.Remove(circle);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // 1. عرض صفحة التعديل (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var circle = await _context.Circles.FindAsync(id);
            if (circle == null) return NotFound();

            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name", circle.TeacherId);
            return View(circle);
        }

        // 2. معالجة حفظ التعديل (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CircleType,Gender,TeacherId,TimingType,StartPrayer,EndPrayer,StartTime,EndTime")] Circle circle, string[] SelectedDays)
        {
            if (id != circle.Id) return NotFound();

            ModelState.Remove("Teacher");
            ModelState.Remove("Students");

            if (ModelState.IsValid)
            {
                try
                {
                    circle.SelectedDays = (SelectedDays != null && SelectedDays.Length > 0)
                        ? string.Join(", ", SelectedDays) : "لم يتم تحديد أيام";

                    _context.Update(circle);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Circles.Any(e => e.Id == circle.Id)) return NotFound();
                    else throw;
                }
            }
            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name", circle.TeacherId);
            return View(circle);
        }

        // 3. دالة جلب التفاصيل لعرضها في الـ Modal
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var circle = await _context.Circles
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (circle == null) return NotFound();

            return PartialView("_DetailsPartial", circle);
        }
    }
}