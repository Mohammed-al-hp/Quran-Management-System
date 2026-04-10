using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم المستخدم")]
        public string Username { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "نوع الحساب (الدور)")]
        public string Role { get; set; } // مثلاً: Admin أو Teacher

        [Display(Name = "حالة الحساب")]
        public bool IsActive { get; set; } = true;
    }
}