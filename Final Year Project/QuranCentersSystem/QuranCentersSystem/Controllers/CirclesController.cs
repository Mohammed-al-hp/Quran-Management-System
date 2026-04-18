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

        // 1. عرض قائمة الحلقات مع الفلترة والبحث
        public async Task<IActionResult> Index(string gender, string circleType)
        {
            // جلب الحلقات مع تضمين بيانات المحفظين والطلاب لحساب العدد
            var query = _context.Circles
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .AsQueryable();

            if (!string.IsNullOrEmpty(gender) && gender != "الكل")
            {
                query = query.Where(c => c.Gender == gender);
            }

            if (!string.IsNullOrEmpty(circleType) && circleType != "الكل")
            {
                query = query.Where(c => c.CircleType == circleType);
            }

            return View(await query.ToListAsync());
        }

        // 2. عرض صفحة إضافة حلقة جديدة
        public IActionResult Create()
        {
            // تجهيز قائمة المحفظين للاختيار منها في الواجهة
            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name");
            return View();
        }

        // 3. معالجة حفظ الحلقة الجديدة (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,TeacherId,CircleType,Gender,TimingType,StartPrayer,EndPrayer,StartTime,EndTime")] Circle circle, string[] SelectedDays)
        {
            // إزالة التحقق من الكائنات المرتبطة لضمان مرور ModelState.IsValid
            ModelState.Remove("Teacher");
            ModelState.Remove("Students");
            ModelState.Remove("SelectedDays");

            if (ModelState.IsValid)
            {
                try
                {
                    // تحويل مصفوفة الأيام المختارة إلى نص مفصول بفاصلة لحفظها
                    if (SelectedDays != null && SelectedDays.Length > 0)
                    {
                        circle.SelectedDays = string.Join(", ", SelectedDays);
                    }

                    _context.Add(circle);
                    await _context.SaveChangesAsync();

                    // العودة للقائمة بعد النجاح
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // إضافة خطأ في حال فشل الحفظ في قاعدة البيانات
                    ModelState.AddModelError("", "حدث خطأ أثناء الاتصال بقاعدة البيانات: " + ex.Message);
                }
            }

            // إذا وصلنا هنا، فهذا يعني وجود خطأ في البيانات المدخلة
            // نعيد ملء قائمة المحفظين لكي لا تظهر فارغة في الواجهة
            ViewBag.TeacherId = new SelectList(_context.Teachers, "Id", "Name", circle.TeacherId);
            return View(circle);
        }

        // 4. حذف الحلقة (إضافي للتكامل)
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
    }
}