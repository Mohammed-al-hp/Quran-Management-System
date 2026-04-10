using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. عرض قائمة المحفظين مع البحث والفرز المتقدم
        public async Task<IActionResult> Index(string searchTerm, string sortBy)
        {
            var query = _context.Teachers
                .Include(t => t.Circles)
                    .ThenInclude(c => c.Students)
                .AsQueryable();

            // الفلترة بالبحث
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) || t.Qualification.Contains(searchTerm));
            }

            // منطق الفرز (Sorting) - تم إصلاح "students" هنا
            query = sortBy switch
            {
                "oldest" => query.OrderBy(t => t.Id),
                // الإصلاح: استخدام SelectMany لجمع الطلاب ثم عدهم ككتلة واحدة
                "students" => query.OrderByDescending(t => t.Circles.SelectMany(c => c.Students).Count()),
                "alphabetical" => query.OrderBy(t => t.Name),
                _ => query.OrderByDescending(t => t.Id), // الأحدث هو الافتراضي
            };

            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchTerm;

            return View(await query.ToListAsync());
        }

        // 1. عرض صفحة الإضافة
        public IActionResult Create()
        {
            // جلب الحلقات لإظهارها في القائمة المنسدلة
            ViewBag.Circles = new SelectList(_context.Circles, "Id", "Name");
            return View();
        }

        // 2. معالجة بيانات الإضافة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher, IFormFile? teacherImage, int? selectedCircleId)
        {
            if (ModelState.IsValid)
            {
                // معالجة رفع الصورة (إذا وُجدت)
                if (teacherImage != null && teacherImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(teacherImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/teachers", fileName);

                    // تأكد من وجود المجلد
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/teachers"));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await teacherImage.CopyToAsync(stream);
                    }
                    teacher.ImagePath = "/images/teachers/" + fileName;
                }

                _context.Add(teacher);
                await _context.SaveChangesAsync();

                // ربط المعلم بالحلقة المختارة (إذا تم اختيار حلقة)
                if (selectedCircleId.HasValue)
                {
                    var circle = await _context.Circles.FindAsync(selectedCircleId);
                    if (circle != null)
                    {
                        circle.TeacherId = teacher.Id;
                        _context.Update(circle);
                        await _context.SaveChangesAsync();
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Circles = new SelectList(_context.Circles, "Id", "Name");
            return View(teacher);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers.FindAsync(id);
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
                    _context.Update(teacher);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TeacherExists(teacher.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(teacher);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var teacher = await _context.Teachers
                .Include(t => t.Circles)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (teacher == null) return NotFound();
            return View(teacher);
        }

        // تحسين منطق الحذف لتجنب أخطاء القيود (Constraint Error)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Circles)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher != null)
            {
                // إذا كانت الحلقات مرتبطة بالمعلم، نقوم بفك الارتباط أولاً
                foreach (var circle in teacher.Circles)
                {
                    circle.TeacherId = null;
                }

                _context.Teachers.Remove(teacher);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool TeacherExists(int id) => _context.Teachers.Any(e => e.Id == id);
    }
}