using System.Collections.Generic;

namespace QuranCentersSystem.Models
{
    public class Circle
    {
        public int Id { get; set; }
        public string Name { get; set; }

        // الربط مع المحفظ
        public int? TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }

        // التعديل: إضافة علاقة الطلاب ليتمكن النظام من عددهم داخل كل حلقة
        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}