using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenters.Core.Entities
{
    public class Student
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "اسم الطالب")]
        public string Name { get; set; }

        // أضف هذا الحقل لحل مشكلة FullName في التقرير
        [Display(Name = "الاسم الكامل")]
        public string FullName => Name;

        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الميلاد")]
        public DateTime BirthDate { get; set; }

        [Display(Name = "رقم هاتف ولي الأمر")]
        public string ParentPhoneNumber { get; set; }

        public bool AgreedToTerms { get; set; }
        public string Status { get; set; } = "نشط";
        public DateTime JoinDate { get; set; } = DateTime.Now;

        [Display(Name = "بريد ولي الأمر")]
        public string? ParentEmail { get; set; }

        [Display(Name = "رمز QR")]
        public string? QrCodeToken { get; set; }

        public int CircleId { get; set; }
        public virtual Circle Circle { get; set; }
        public int? ParentId { get; set; } // جعلناه اختياري لضمان عدم تعطل البيانات القديمة
        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; }
        // أضف هذا السطر لحل أخطاء "Payments" في الـ Controller والـ Index
        public virtual ICollection<Payment> Payments { get; set; }

        // 🌟 الحل الرئيسي: إضافة هذه الأسطر لربط الجداول
        public string? ParentEmail { get; set; } // أضف هذا السطر لربط الطالب بحساب ولي الأمر
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Memorization> Memorizations { get; set; } = new List<Memorization>();
        public virtual ICollection<PointsLedger> PointsLedgers { get; set; } = new List<PointsLedger>();
        public virtual ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
    }
}