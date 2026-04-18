using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCenters.Core.Entities
{
    /// <summary>
    /// نموذج المدفوعات - يمثل دفعة مالية مرتبطة بطالب معين
    /// </summary>
    public class Payment
    {
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }

        [Required(ErrorMessage = "المبلغ مطلوب")]
        [Display(Name = "المبلغ")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "تاريخ الدفع")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Display(Name = "نوع الدفع")]
        public string PaymentType { get; set; }

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        [Display(Name = "تم بواسطة")]
        public string CreatedBy { get; set; }
    }
}
