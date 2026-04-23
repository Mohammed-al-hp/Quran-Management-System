using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Core.Entities
{
    /// <summary>
    /// نموذج إعدادات النظام - إعدادات قابلة للتكوين من لوحة التحكم
    /// </summary>
    public class SystemSetting
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "المفتاح")]
        public string Key { get; set; } // e.g. "center_name", "academic_year", "max_students_per_circle"

        [Display(Name = "القيمة")]
        public string Value { get; set; }

        [Display(Name = "الفئة")]
        [MaxLength(50)]
        public string Category { get; set; } // General, Gamification, Academic, Notifications

        [Display(Name = "الوصف")]
        public string Description { get; set; }
    }
}
