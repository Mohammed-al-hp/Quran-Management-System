using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    /// <summary>
    /// نموذج الإشعارات - يمثل إشعاراً داخلياً يُرسل للمستخدم
    /// يُستخدم لإعلام أولياء الأمور بحضور أبنائهم وإعلام الطلاب بالمهام الجديدة
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "معرف المستخدم")]
        public string UserId { get; set; }

        [Required]
        [Display(Name = "العنوان")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "الرسالة")]
        public string Message { get; set; }

        [Display(Name = "النوع")]
        public string Type { get; set; } // Attendance, Task, Grade

        [Display(Name = "تمت القراءة")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "معرف الكيان المرتبط")]
        public int? RelatedEntityId { get; set; }
    }
}
