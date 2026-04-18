using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCenters.Core.Entities
{
    public class PointsLedger
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public int Points { get; set; }
        public string Reason { get; set; }
        public DateTime DateAwarded { get; set; } = DateTime.Now;
    }
}
