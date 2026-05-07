namespace QuranCentersSystem.Models
{
    public class DashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalCircles { get; set; }
        public int TodayAttendance { get; set; }

        // أضفنا هذه الحقول لنقل بيانات الرسم البياني أيضاً عبر الموديل
        public int ExcellentCount { get; set; }
        public int VeryGoodCount { get; set; }
        public int GoodCount { get; set; }
        public int FairCount { get; set; }
    }
}