using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PaymentsController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IStudentService _studentService;

        public PaymentsController(IPaymentService paymentService, IStudentService studentService)
        {
            _paymentService = paymentService;
            _studentService = studentService;
        }

        // عرض قائمة المدفوعات
        public async Task<IActionResult> Index()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
            return View(payments);
        }

        // صفحة إضافة دفعة جديدة
        public async Task<IActionResult> Create()
        {
            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.StudentId = new SelectList(students, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                await _paymentService.CreatePaymentAsync(payment, User.Identity?.Name ?? "System");
                return RedirectToAction(nameof(Index));
            }
            var students = await _studentService.GetAllStudentsAsync();
            ViewBag.StudentId = new SelectList(students, "Id", "Name", payment.StudentId);
            return View(payment);
        }
    }
}