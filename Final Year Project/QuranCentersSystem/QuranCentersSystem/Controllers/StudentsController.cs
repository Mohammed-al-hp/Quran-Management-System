using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using Rotativa.AspNetCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    [Authorize]
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        // 1. أضف هذا السطر هنا (تعريف المتغيرات)
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // 2. قم بتعديل هذا الجزء (الـ Constructor) لاستقبال الـ UserManager
        public StudentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? circleId)
        {
            ViewBag.Circles = await _context.Circles.ToListAsync();
            ViewBag.SelectedCircle = circleId;

            var studentsQuery = _context.Students.Include(s => s.Circle).AsQueryable();

            if (circleId.HasValue)
            {
                studentsQuery = studentsQuery.Where(s => s.CircleId == circleId.Value);
            }

            var students = await studentsQuery.ToListAsync();
            return View(students);
        }

        // 🌟 دالة عرض تفاصيل الطالب وسجل إنجازاته
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.StudentAchievements) // جلب سجل الإنجازات لعرضها في الجدول
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return View(student);
        }

        public async Task<IActionResult> PrintStudentReport(int id)
        {
            var student = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.StudentAchievements) // تأكد من استخدام الاسم الصحيح للعلاقة (Achievements)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (student == null) return NotFound();

            return new ViewAsPdf("StudentReportPDF", student)
            {
                FileName = $"Report_{student.Name}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--encoding utf-8"
            };
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name");
            ViewBag.ParentId = new SelectList(_context.Set<Parent>(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Phone,BirthDate,ParentPhoneNumber,AgreedToTerms,CircleId,ParentId,ParentEmail,Username,Password,CurrentSurah,ParentRelation,NewParentName,NewParentPhone")] Student student, IFormFile? StudentPhoto)
        {
            ModelState.Remove("AgreedToTerms");
            ModelState.Remove("StudentPhoto");

            if (ModelState.IsValid)
            {
                // 1. معالجة الصورة الشخصية
                if (StudentPhoto != null && StudentPhoto.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(StudentPhoto.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/students", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await StudentPhoto.CopyToAsync(stream);
                    }
                    student.ImagePath = "/uploads/students/" + fileName;
                }

                // 2. معالجة ولي الأمر (تحديث: ضمان عدم توقف الحفظ بسبب البريد الإلكتروني)
                if (!string.IsNullOrEmpty(student.NewParentName))
                {
                    var newParent = new Parent
                    {
                        Name = student.NewParentName,
                        Phone = student.NewParentPhone
                    };
                    _context.Add(newParent);
                    await _context.SaveChangesAsync();
                    student.ParentId = newParent.Id;
                }

                student.Status = "نشط";
                student.JoinDate = DateTime.Now;

                // 3. حفظ بيانات الطالب في جدول Students
                _context.Add(student);
                await _context.SaveChangesAsync();

                // 4. 🌟 الربط التلقائي: إنشاء حساب مستخدم في Identity 🌟
                if (!string.IsNullOrEmpty(student.Username) && !string.IsNullOrEmpty(student.Password))
                {
                    var userAccount = new ApplicationUser
                    {
                        UserName = student.Username,
                        Email = student.ParentEmail ?? $"{student.Username}@system.com",
                        EmailConfirmed = true,
                        IsActive = true,
                        // ✅ الحل: إعطاء قيمة للحقل المطلوب في قاعدة البيانات لتجنب خطأ NULL
                        Role = "Student"
                    };

                    var result = await _userManager.CreateAsync(userAccount, student.Password);

                    if (result.Succeeded)
                    {
                        // إسناد الدور رسمياً في نظام الصلاحيات
                        await _userManager.AddToRoleAsync(userAccount, "Student");
                    }
                    else
                    {
                        // في حال فشل إنشاء الحساب (مثلاً كلمة المرور ضعيفة)
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", "خطأ في إنشاء حساب المستخدم: " + error.Description);
                        }
                        // ملاحظة: قد ترغب في حذف سجل الطالب هنا إذا كان حساب المستخدم ضرورياً جداً
                    }
                }

                TempData["SuccessMessage"] = $"تمت إضافة الطالب {student.Name} بنجاح!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            ViewBag.ParentId = new SelectList(_context.Set<Parent>(), "Id", "Name", student.ParentId);
            return View(student);
        }
        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var student = await _context.Students.FindAsync(id);
            if (student == null) return NotFound();

            // تجهيز القائمة المنسدلة للحلقات لتظهر في صفحة التعديل
            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", student.CircleId);
            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentPhoneNumber,CircleId,Status,JoinDate")] Student studentForm)
        {
            if (id != studentForm.Id) return NotFound();

            ModelState.Remove("Phone");
            ModelState.Remove("BirthDate");
            ModelState.Remove("ParentId");
            ModelState.Remove("AgreedToTerms"); // مهم جداً لأننا أزلنا هذا الحقل

            if (ModelState.IsValid)
            {
                try
                {
                    var existingStudent = await _context.Students.FindAsync(id);
                    if (existingStudent == null) return NotFound();

                    existingStudent.Name = studentForm.Name;
                    existingStudent.CircleId = studentForm.CircleId;
                    existingStudent.ParentPhoneNumber = studentForm.ParentPhoneNumber;
                    existingStudent.Status = studentForm.Status;
                    existingStudent.JoinDate = studentForm.JoinDate;

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Students.Any(e => e.Id == studentForm.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["CircleId"] = new SelectList(_context.Circles, "Id", "Name", studentForm.CircleId);
            return View(studentForm);
        }

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