using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class Circle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم الحلقة مطلوب")]
        public string Name { get; set; }

        // التصفية والأنواع
        public string CircleType { get; set; } // رقم أو نوع الحلقة
        public string Gender { get; set; }     // ذكر / أنثى

        // الربط مع المحفظ
        public int? TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }

        // المواعيد والأيام
        public string SelectedDays { get; set; }
        public string TimingType { get; set; }
        public string StartPrayer { get; set; }
        public string EndPrayer { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }

        // علاقة الطلاب (للحصول على العدد تلقائياً)
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}