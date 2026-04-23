using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Core.Entities
{
    /// <summary>
    /// نموذج المهام - مهام يُنشئها المعلم للطلاب (حفظ، مراجعة، بحث)
    /// </summary>
    public class Assignment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان المهمة مطلوب")]
        [Display(Name = "عنوان المهمة")]
        [MaxLength(200)]
        public string Title { get; set; }

        [Display(Name = "الوصف")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "الحلقة")]
        public int CircleId { get; set; }
        public virtual Circle Circle { get; set; }

        [Display(Name = "المعلم")]
        public int? TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }

        [Display(Name = "تاريخ التكليف")]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "تاريخ التسليم مطلوب")]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ التسليم")]
        public DateTime DueDate { get; set; }

        [Display(Name = "نوع المهمة")]
        public string AssignmentType { get; set; } // حفظ، مراجعة، بحث

        [Display(Name = "السورة المستهدفة")]
        public string TargetSurah { get; set; }

        [Display(Name = "من آية")]
        public int? FromAyah { get; set; }

        [Display(Name = "إلى آية")]
        public int? ToAyah { get; set; }

        [Display(Name = "الحالة")]
        public string Status { get; set; } = "نشطة"; // نشطة، مغلقة

        public virtual ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
