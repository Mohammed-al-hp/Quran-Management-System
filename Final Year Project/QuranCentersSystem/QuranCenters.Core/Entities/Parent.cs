using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Core.Entities
{
    public class Parent
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Phone { get; set; }
        // أضف هذا السطر لحل خطأ السطر 32 في الـ Controller
        public string Email { get; set; }

        public virtual ICollection<Student> Students { get; set; } = new List<Student>();
    }
}