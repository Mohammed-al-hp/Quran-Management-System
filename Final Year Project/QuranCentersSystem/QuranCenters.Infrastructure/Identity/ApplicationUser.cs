using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Infrastructure.Identity
{
    /// <summary>
    /// نموذج المستخدم المخصص - يرث من IdentityUser ويضيف خصائص إضافية
    /// يدعم تعيين الدور والتحكم في حالة الحساب
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "نوع الحساب (الدور)")]
        public string Role { get; set; } = "Student";

        [Display(Name = "حالة الحساب")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "الاسم الكامل")]
        public string? FullName { get; set; }
    }
}