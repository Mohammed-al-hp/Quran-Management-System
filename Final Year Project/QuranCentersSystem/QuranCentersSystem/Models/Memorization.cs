using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCentersSystem.Models
{
    public class StudentAchievement
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Display(Name = "التاريخ")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Display(Name = "نوع الإنجاز")]
        public AchievementType Type { get; set; }

        [Required(ErrorMessage = "اسم السورة مطلوب")]
        [Display(Name = "من سورة")]
        public string SurahStart { get; set; }

        [Display(Name = "من آية")]
        public int AyahStart { get; set; }

        [Required(ErrorMessage = "اسم السورة مطلوب")]
        [Display(Name = "إلى سورة")]
        public string SurahEnd { get; set; }

        [Display(Name = "إلى آية")]
        public int AyahEnd { get; set; }

        [Display(Name = "التقدير/العلامة")]
        public string Grade { get; set; } // مثال: ممتاز، جيد جداً

        [Display(Name = "ملاحظات المحفظ")]
        public string? TeacherNotes { get; set; }
    }

    public enum AchievementType
    {
        [Display(Name = "حفظ جديد")] Memorization,
        [Display(Name = "مراجعة صغرى")] MinorRevision,
        [Display(Name = "مراجعة كبرى")] MajorRevision
    }
}