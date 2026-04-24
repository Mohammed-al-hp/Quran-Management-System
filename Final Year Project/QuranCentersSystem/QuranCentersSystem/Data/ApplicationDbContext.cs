using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Data
{
    // التعديل 1: أضف <ApplicationUser> هنا
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
        // التعديل 2: يفضل حذف هذا السطر لأن IdentityDbContext يعرفه تلقائياً كـ Users
        // public DbSet<ApplicationUser> ApplicationUsers { get; set; } 

        public DbSet<MemorizationQuestion> MemorizationQuestions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }
        public DbSet<Memorization> Memorizations { get; set; }

        // التعديل 3: إضافة هذه الدالة بالكامل لحل مشكلة الـ Discriminator
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // حل مشكلة الخطأ: Invalid column name 'Discriminator'
            builder.Entity<ApplicationUser>()
                .HasBaseType((Type)null);

            // اختياري: يمكنك هنا تحديد أسماء الجداول لتكون بسيطة (مثل Users بدل AspNetUsers)
            builder.Entity<ApplicationUser>(entity => {
                entity.ToTable(name: "Users");
            });
        }
    }
}