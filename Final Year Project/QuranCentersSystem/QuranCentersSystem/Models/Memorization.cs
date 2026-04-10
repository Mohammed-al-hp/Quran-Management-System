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
        public string DayName { get; set; } // الأحد، الإثنين... الخ

        // 🟢 أعدنا الحقول القديمة هنا بصيغة تقبل الفراغ (Null) لتختفي الأخطاء
        [Display(Name = "اسم السورة")]
        public string SurahName { get; set; }

        [Display(Name = "آية البداية")]
        public int? FromAyah { get; set; }

        [Display(Name = "آية النهاية")]
        public int? ToAyah { get; set; }

        // 🔵 حقول المتابعة والأسئلة الخاصة بك
        [Display(Name = "السورة الحالية")]
        public string CurrentSurah { get; set; }

        [Display(Name = "مقدار المتابعة")]
        public string Scope { get; set; } // مثلاً: من البقرة 1 إلى 50

        [Required(ErrorMessage = "يرجى تحديد التقييم")]
        [Display(Name = "التقييم")]
        public string Grade { get; set; } // ممتاز، جيد جداً...

        [Display(Name = "ملاحظات المعلم")]
        public string Notes { get; set; }

        // قائمة الأسئلة المرتبطة
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