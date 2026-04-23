using QuranCentersSystem.Models;

public class TeacherAttendance
{
    public int Id { get; set; }
    public DateTime Date { get; set; } = DateTime.Now.Date;
    public string Status { get; set; } // حاضر، غائب، متأخر، عذر
    public double WorkHours { get; set; } // عدد ساعات العمل
    public string Notes { get; set; }
    public int TeacherId { get; set; }
    public virtual Teacher Teacher { get; set; }
}