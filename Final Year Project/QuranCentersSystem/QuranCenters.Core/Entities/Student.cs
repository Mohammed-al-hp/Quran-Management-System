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

        // Navigation Properties
        public int CircleId { get; set; }
        public virtual Circle Circle { get; set; }

        public int? ParentId { get; set; }
        [ForeignKey("ParentId")]
        public virtual Parent Parent { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Memorization> Memorizations { get; set; } = new List<Memorization>();
        public virtual ICollection<PointsLedger> PointsLedgers { get; set; } = new List<PointsLedger>();
        public virtual ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
        public virtual ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
        public virtual ICollection<KhatmProgress> KhatmProgresses { get; set; } = new List<KhatmProgress>();
    }
}