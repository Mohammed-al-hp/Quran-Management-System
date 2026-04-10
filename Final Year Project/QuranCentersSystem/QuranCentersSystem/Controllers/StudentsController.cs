using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuranCentersSystem.Controllers
{
    [Authorize] // قفل الشاشات وجعلها تتطلب تسجيل الدخول
    public class StudentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض قائمة الطلاب مع دعم "الفلترة حسب الحلقة"
        public async Task<IActionResult> Index(int? circleId)
        {
            // جلب قائمة الحلقات لعرضها في القائمة المنسدلة في الواجهة
            ViewBag.Circles = await _context.Circles.ToListAsync();
            ViewBag.SelectedCircle = circleId;

            // بناء الاستعلام الأساسي مع جلب بيانات الحلقة المرتبطة
            var studentsQuery = _context.Students.Include(s => s.Circle).AsQueryable();

            // تطبيق الفلترة إذا تم اختيار حلقة معينة
            if (circleId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.CircleId == circleId.Value);
            }

            var students = await studentsQuery.ToListAsync();
            return View(students);
        }

        // 2. عرض صفحة إضافة طالب جديد (تجهيز القائمة المنسدلة)
        public IActionResult Create()
        {
            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name");
            return View();
        }

        // 3. استقبال بيانات الطالب الجديد وحفظها
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,BirthDate,ParentPhoneNumber,AgreedToTerms,CircleId")] Student student)
        {
            if (!student.AgreedToTerms)
            {
                ModelState.AddModelError("AgreedToTerms", "يجب الموافقة على شروط الانضمام للمركز");
            }

            if (ModelState.IsValid)
            {
                student.Status = "نشط";
                student.JoinDate = DateTime.Now;

                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            return View(student);
        }

        // 4. عرض صفحة تعديل بيانات طالب
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            return View(student);
        }

        // 5. حفظ التعديلات
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Phone,BirthDate,ParentPhoneNumber,AgreedToTerms,CircleId,Status,JoinDate")] Student student)
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
            return View(student);
        }

        // 6. حذف طالب (للمدير فقط - يتم استدعاؤه مباشرة أو عبر صفحة تأكيد)
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

        // تنفيذ الحذف النهائي
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