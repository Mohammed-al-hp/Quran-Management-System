using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCentersSystem.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "اسم الطالب")]
        public string Name { get; set; }

        [Display(Name = "الاسم الكامل")]
        public string FullName => Name;

        [Display(Name = "رقم الهاتف")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الميلاد")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "رقم هاتف ولي الأمر")]
        public string? ParentPhoneNumber { get; set; }

        // تم تعديله ليكون اختيارياً لعدم عرقلة الحفظ
        public bool? AgreedToTerms { get; set; }

        public string Status { get; set; } = "نشط";
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "يجب اختيار حلقة")]
        public int CircleId { get; set; }
        public virtual Circle? Circle { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Parent? Parent { get; set; }

        public virtual ICollection<Payment>? Payments { get; set; }
        public string? ParentEmail { get; set; }

        public string? ImagePath { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? CurrentSurah { get; set; }

        [NotMapped]
        public string? NewParentName { get; set; }
        [NotMapped]
        public string? NewParentPhone { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<StudentAchievement> StudentAchievements { get; set; } = new List<StudentAchievement>();
    }
}