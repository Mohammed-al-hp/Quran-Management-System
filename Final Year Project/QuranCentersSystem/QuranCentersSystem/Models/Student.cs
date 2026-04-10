using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى إدخال اسم الطالب")]
        [Display(Name = "اسم الطالب")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "يرجى إدخال رقم هاتف الطالب")]
        [Display(Name = "هاتف الطالب")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "يرجى تحديد تاريخ الانضمام")]
        [Display(Name = "تاريخ الانضمام")]
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "يرجى تحديد حالة الطالب")]
        [Display(Name = "الحالة")]
        public string? Status { get; set; } = "نشط";

        // --- الحقول الجديدة المضافة بناءً على طلبك ---

        [Required(ErrorMessage = "يرجى إدخال تاريخ ميلاد الطالب")]
        [Display(Name = "مواليد الطالب")]
        [DataType(DataType.Date)]
        public DateTime BirthDate { get; set; }

        [Required(ErrorMessage = "يرجى إدخال رقم هاتف ولي الأمر")]
        [Display(Name = "هاتف ولي الأمر")]
        public string? ParentPhoneNumber { get; set; }

        [Display(Name = "موافقة ولي الأمر والطالب على الشروط")]
        public bool AgreedToTerms { get; set; }

        // ربط الطالب بالحلقة
        [Display(Name = "الحلقة")]
        public int CircleId { get; set; }
        public virtual Circle? Circle { get; set; }
    }
}