using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;

[Route("api/[controller]")]
[ApiController]
public class TeachersApiController : ControllerBase
{
    private readonly ApplicationDbContext _context; // استخدام نفس سياق البيانات

    public TeachersApiController(ApplicationDbContext context)
    {
        _context = context;
    }

    // جلب قائمة المحفظين لعرضها في التطبيق
    [HttpGet]
    public async Task<IActionResult> GetTeachers()
    {
        var teachers = await _context.Teachers.ToListAsync();
        return Ok(teachers); // إرسال البيانات بصيغة JSON
    }
    // GET: api/TeachersApi/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        // جلب الأعداد الحقيقية من جداول قاعدة البيانات الخاصة بك
        var stats = new
        {
            teachersCount = await _context.Teachers.CountAsync(),
            circlesCount = await _context.Circles.CountAsync(),
            studentsCount = await _context.Students.CountAsync(),
            // يمكنك لاحقاً جعل السعة القصوى متغيرة من الإعدادات
            maxCapacity = 52
        };

        return Ok(stats);
    }
}