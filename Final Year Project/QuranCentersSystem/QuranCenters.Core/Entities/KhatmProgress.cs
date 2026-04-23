using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenters.Core.Entities
{
    /// <summary>
    /// نموذج تقدم الختمة - يتتبع رحلة كل طالب في حفظ القرآن الكريم كاملاً
    /// يدعم تتبع ختمات متعددة (الختمة الأولى، الثانية، إلخ)
    /// </summary>
    public class KhatmProgress
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        [Display(Name = "رقم الختمة")]
        public int KhatmNumber { get; set; } = 1; // الختمة الأولى، الثانية...

        [Display(Name = "تاريخ البدء")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [Display(Name = "تاريخ الإكمال")]
        [DataType(DataType.Date)]
        public DateTime? CompletionDate { get; set; }

        [Display(Name = "إجمالي الصفحات المحفوظة")]
        [Column(TypeName = "decimal(7,2)")]
        public decimal TotalPagesMemorized { get; set; }

        [Display(Name = "السورة الحالية")]
        public string CurrentSurah { get; set; }

        [Display(Name = "الآية الحالية")]
        public int CurrentAyah { get; set; }

        [Display(Name = "الحالة")]
        public string Status { get; set; } = "جاري"; // جاري، مكتمل

        /// <summary>
        /// إجمالي صفحات المصحف = 604
        /// </summary>
        public const decimal TotalQuranPages = 604m;

        [Display(Name = "نسبة الإنجاز")]
        [NotMapped]
        public decimal CompletionPercentage =>
            TotalQuranPages > 0 ? Math.Round(TotalPagesMemorized / TotalQuranPages * 100, 1) : 0;
    }
}
