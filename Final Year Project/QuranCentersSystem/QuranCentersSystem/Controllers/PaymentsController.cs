using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Microsoft.AspNetCore.Authorization;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin")] // للمدير فقط
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context) => _context = context;

        // عرض قائمة المدفوعات
        public async Task<IActionResult> Index()
        {
            var payments = _context.Payments.Include(p => p.Student);
            return View(await payments.ToListAsync());
        }

        // شاشة إضافة دفعة جديدة
        public IActionResult Create()
        {
            ViewBag.StudentId = new SelectList(_context.Students, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                payment.CreatedBy = User.Identity.Name; // تسجيل من قام بالعملية
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StudentId = new SelectList(_context.Students, "Id", "Name", payment.StudentId);
            return View(payment);
        }
    }
}