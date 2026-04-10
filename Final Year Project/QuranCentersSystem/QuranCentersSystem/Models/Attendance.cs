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

        public string Status { get; set; } // (حاضر، غائب، مستأذن)

        public string Notes { get; set; }

        [Required]
        public int StudentId { get; set; }
        public virtual Student Student { get; set; }
    }
}