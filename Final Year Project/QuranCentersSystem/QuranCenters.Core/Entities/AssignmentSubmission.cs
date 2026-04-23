using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Core.Entities
{
    /// <summary>
    /// نموذج تسليم المهام - يمثل تسليم طالب لمهمة معينة وتقييم المعلم لها
    /// </summary>
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }
        public virtual Assignment Assignment { get; set; }

        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        [Display(Name = "تاريخ التسليم")]
        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        [Display(Name = "الحالة")]
        public string Status { get; set; } = "معلقة"; // معلقة، مقبولة، مرفوضة

        [Display(Name = "ملاحظات المعلم")]
        public string TeacherNotes { get; set; }

        [Display(Name = "التقييم")]
        public string Grade { get; set; }
    }
}
