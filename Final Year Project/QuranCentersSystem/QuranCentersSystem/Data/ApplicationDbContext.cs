using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }

        // التعديل الأساسي: استخدام سجل الإنجاز الجديد
        public DbSet<StudentAchievement> StudentAchievements { get; set; }

        // إذا كنت لا تزال تستخدم جدول الأسئلة المنفصل
      //  public DbSet<StudentAchievementQuestion> StudentAchievementQuestions { get; set; }

        public DbSet<GroupAchievement> GroupAchievements { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // حل مشكلة الـ Discriminator وتخصيص جداول Identity
            builder.Entity<ApplicationUser>(entity =>
            {
                entity.ToTable(name: "Users");
                entity.HasBaseType((Type)null); // لضمان عدم البحث عن Discriminator
            });

            // ضبط علاقات الطلاب مع الإنجازات (حذف تلقائي عند حذف الطالب)
            // داخل OnModelCreating
            builder.Entity<StudentAchievement>()
                .HasOne(s => s.Student)
                .WithMany(a => a.StudentAchievements) // تأكد أن هذا الاسم مطابق لما هو موجود في موديل Student
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}