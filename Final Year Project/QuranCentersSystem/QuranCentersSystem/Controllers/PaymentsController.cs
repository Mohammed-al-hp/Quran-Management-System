using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin")] // ?????? ???
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context) => _context = context;

        // ??? ????? ?????????
        public async Task<IActionResult> Index()
        {
            var payments = _context.Payments.Include(p => p.Student);
            return View(await payments.ToListAsync());
        }

        // ???? ????? ???? ?????
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
                payment.CreatedBy = User.Identity.Name; // ????? ?? ??? ????????
                _context.Add(payment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.StudentId = new SelectList(_context.Students, "Id", "Name", payment.StudentId);
            return View(payment);
        }
    }
}