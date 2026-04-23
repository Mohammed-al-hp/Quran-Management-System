using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuranCentersSystem.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى اختيار الطالب")]
        [Display(Name = "الطالب")]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Required(ErrorMessage = "يرجى إدخال المبلغ")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "المبلغ")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "تاريخ الدفع")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "يرجى تحديد نوع الدفعة")]
        [Display(Name = "نوع الدفعة")]
        public string PaymentType { get; set; } // مثال: رسوم شهرية، كتب، نشاط

        [Display(Name = "ملاحظات")]
        public string Notes { get; set; }

        [Display(Name = "سُجل بواسطة")]
        public string CreatedBy { get; set; }
    }
}