using System.Collections.Generic;

namespace QuranCenters.Application.DTOs
{
    public class LeaderboardViewModel
    {
        public List<StudentRankDto> Rankings { get; set; } = new List<StudentRankDto>();
    }

    public class StudentRankDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string CircleName { get; set; }
        public int TotalPoints { get; set; }
        public int Rank { get; set; }
        public int BadgeCount { get; set; }
    }
}
