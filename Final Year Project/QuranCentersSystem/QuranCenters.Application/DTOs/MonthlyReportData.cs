using System.Collections.Generic;
using System.Threading.Tasks;
using QuranCenters.Core.Entities;

namespace QuranCenters.Application.DTOs
{
    public class MonthlyReportData
    {
        public string StudentName { get; set; }
        public string CircleName { get; set; }
        public string TeacherName { get; set; }
        public string Month { get; set; }
        public string ReportDate { get; set; }

        public int TotalDays { get; set; }
        public int PresentDays { get; set; }
        public int AbsentDays { get; set; }
        public string AttendanceRate { get; set; }

        public int TotalSessions { get; set; }
        public int ExcellentCount { get; set; }
        public int VeryGoodCount { get; set; }
        public int GoodCount { get; set; }
        public int FairCount { get; set; }

        public List<Attendance> Attendances { get; set; }
        public List<Memorization> Memorizations { get; set; }

        public string OverallAssessment { get; set; }
    }
}
