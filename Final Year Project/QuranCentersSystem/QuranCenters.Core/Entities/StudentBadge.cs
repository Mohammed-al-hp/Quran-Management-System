using System;

namespace QuranCenters.Core.Entities
{
    public class StudentBadge
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public string BadgeName { get; set; }
        public string IconUrl { get; set; }
        public DateTime DateEarned { get; set; } = DateTime.Now;
    }
}
