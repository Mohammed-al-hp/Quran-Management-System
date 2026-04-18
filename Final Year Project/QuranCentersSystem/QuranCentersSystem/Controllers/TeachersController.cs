using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin")] // ???? ?????? ???
    public class TeachersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. ??? ????? ???????? ?? ????? ?????? ???????
        public async Task<IActionResult> Index(string searchTerm, string sortBy)
        {
            var query = _context.Teachers
                .Include(t => t.Circles)
                    .ThenInclude(c => c.Students)
                .AsQueryable();

            // ??????? ??????
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) || t.Qualification.Contains(searchTerm));
            }

            // ???? ????? (Sorting) - ?? ????? "students" ???
            query = sortBy switch
            {
                "oldest" => query.OrderBy(t => t.Id),
                // ???????: ??????? SelectMany ???? ?????? ?? ???? ????? ?????
                "students" => query.OrderByDescending(t => t.Circles.SelectMany(c => c.Students).Count()),
                "alphabetical" => query.OrderBy(t => t.Name),
                _ => query.OrderByDescending(t => t.Id), // ?????? ?? ?????????
            };

            ViewBag.CurrentSort = sortBy;
            ViewBag.CurrentSearch = searchTerm;

            return View(await query.ToListAsync());
        }

        // 1. ??? ???? ???????
        public IActionResult Create()
        {
            // ??? ??????? ???????? ?? ??????? ????????
            ViewBag.Circles = new SelectList(_context.Circles, "Id", "Name");
            return View();
        }

        // 2. ?????? ?????? ???????
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Teacher teacher, IFormFile? teacherImage, int? selectedCircleId)
        {
            if (ModelState.IsValid)
            {
                // ?????? ??? ?????? (??? ?????)
                if (teacherImage != null && teacherImage.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(teacherImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/teachers", fileName);

                    // ???? ?? ???? ??????
                    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/teachers"));

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await teacherImage.CopyToAsync(stream);
                    }
                    teacher.ImagePath = "/images/teachers/" + fileName;
                }

                _context.Add(teacher);
                await _context.SaveChangesAsync();

                // ??? ?????? ??????? ???????? (??? ?? ?????? ????)
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

        // ????? ???? ????? ????? ????? ?????? (Constraint Error)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Circles)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher != null)
            {
                // ??? ???? ??????? ?????? ???????? ???? ??? ???????? ?????
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