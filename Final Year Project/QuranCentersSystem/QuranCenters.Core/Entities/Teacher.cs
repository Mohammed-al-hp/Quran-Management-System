using System.Collections.Generic;

namespace QuranCenters.Core.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Qualification { get; set; }
        // الحقول الجديدة
        public DateTime? BirthDate { get; set; } // تاريخ الميلاد
        public string? ImagePath { get; set; }   // مسار الصورة
        // التعديل: إضافة علاقة الحلقات ليتمكن المتحكم من جلبها وحساب الطلاب
        public virtual ICollection<Circle> Circles { get; set; } = new List<Circle>();
    }
}