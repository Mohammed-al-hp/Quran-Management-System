using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class Memorization
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [Required(ErrorMessage = "يرجى تحديد التاريخ")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "اليوم")]
        public string DayName { get; set; }

        // 🌟 الإضافة الأولى: نوع التسميع (مهم جداً لفرز البيانات لاحقاً)
        [Display(Name = "نوع الإنجاز")]
        public string Type { get; set; } // (حفظ جديد، مراجعة قريبة، مراجعة بعيدة)

        [Display(Name = "اسم السورة")]
        public string SurahName { get; set; }

        [Display(Name = "آية البداية")]
        public int? FromAyah { get; set; }

        [Display(Name = "آية النهاية")]
        public int? ToAyah { get; set; }

        [Display(Name = "السورة الحالية")]
        public string CurrentSurah { get; set; }

        [Display(Name = "مقدار المتابعة")]
        public string Scope { get; set; }

        [Required(ErrorMessage = "يرجى تحديد التقييم")]
        [Display(Name = "التقييم")]
        public string Grade { get; set; }

        // 🌟 الإضافة الثانية: لتوليد رسوم بيانية دقيقة عن تطور الطالب
        [Display(Name = "عدد الأخطاء")]
        public int MistakesCount { get; set; }

        [Display(Name = "ملاحظات المعلم")]
        public string Notes { get; set; }

        public List<MemorizationQuestion> Questions { get; set; } = new List<MemorizationQuestion>();
    }

    public class MemorizationQuestion
    {
        public int Id { get; set; }
        public int MemorizationId { get; set; }
        public Memorization Memorization { get; set; }

        [Required(ErrorMessage = "يرجى كتابة نص السؤال")]
        public string QuestionText { get; set; }

        public string StudentAnswer { get; set; }
    }
}