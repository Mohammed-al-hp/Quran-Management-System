using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class Attendance
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Now.Date;

        // جعلنا الحالة تقبل قيمة فارغة برمجياً لتجنب مشاكل التحقق قبل الحفظ
        public string? Status { get; set; }

        public int DelayMinutes { get; set; }

        // إضافة علامة الاستفهام لتعريف المودل أنها قد تكون فارغة في الواجهة
        public string? Notes { get; set; }

        [Required]
        public int StudentId { get; set; }
        public virtual Student? Student { get; set; }
    }
}