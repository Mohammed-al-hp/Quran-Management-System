using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    /// <summary>
    /// متحكم الحلقات - إدارة حلقات التحفيظ (للمدير فقط)
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize(Roles = "Admin")]
    public class CirclesController : Controller
    {
        private readonly ICircleService _circleService;
        private readonly ITeacherService _teacherService;

        public CirclesController(ICircleService circleService, ITeacherService teacherService)
        {
            _circleService = circleService;
            _teacherService = teacherService;
        }

        // 1. عرض قائمة الحلقات
        public async Task<IActionResult> Index()
        {
            var circles = await _circleService.GetAllCirclesAsync();
            return View(circles);
        }

        // 2. عرض صفحة إضافة حلقة جديدة
        public async Task<IActionResult> Create()
        {
            var teachers = await _teacherService.GetAllTeachersAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "Name");
            return View();
        }

        // 3. حفظ بيانات الحلقة الجديدة
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,TeacherId")] Circle circle)
        {
            // أضف هذه الأسطر لإخبار السيرفر بأن هذه الحقول ليست ضرورية للتحقق الآن
            ModelState.Remove("Teacher");
            ModelState.Remove("Students");

            if (ModelState.IsValid)
            {
                await _circleService.CreateCircleAsync(circle);
                return RedirectToAction(nameof(Index));
            }

            var teachers = await _teacherService.GetAllTeachersAsync();
            ViewBag.TeacherId = new SelectList(teachers, "Id", "Name", circle.TeacherId);
            return View(circle);
        }
    }
}