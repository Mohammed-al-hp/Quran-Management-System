using System;
using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class GroupAchievement
    {
        public int Id { get; set; }
        public int CircleId { get; set; }
        public DateTime Date { get; set; } = DateTime.Today;
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}