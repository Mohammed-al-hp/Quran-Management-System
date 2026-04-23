using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Core.Entities
{
    /// <summary>
    /// نموذج الرسائل - رسائل مباشرة بين المعلمين وأولياء الأمور
    /// </summary>
    public class Message
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "معرف المرسل")]
        public string SenderId { get; set; }

        [Required]
        [Display(Name = "معرف المستقبل")]
        public string ReceiverId { get; set; }

        [Required]
        [Display(Name = "الموضوع")]
        [MaxLength(200)]
        public string Subject { get; set; }

        [Required]
        [Display(Name = "المحتوى")]
        public string Content { get; set; }

        [Display(Name = "تمت القراءة")]
        public bool IsRead { get; set; } = false;

        [Display(Name = "تاريخ الإرسال")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [Display(Name = "الطالب المرتبط")]
        public int? RelatedStudentId { get; set; }
        public virtual Student RelatedStudent { get; set; }
    }
}
