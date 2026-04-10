using System.ComponentModel.DataAnnotations;

namespace QuranCentersSystem.Models
{
    public class Level
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}