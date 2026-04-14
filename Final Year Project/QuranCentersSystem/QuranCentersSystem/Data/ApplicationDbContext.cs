using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Data
{
    /// <summary>
    /// سياق قاعدة البيانات الرئيسي - يدير جميع الجداول وعلاقاتها
    /// يرث من IdentityDbContext لدعم نظام الهوية والمصادقة
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<MemorizationQuestion> MemorizationQuestions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Memorization> Memorizations { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // تكوين جدول Parent للحفاظ على التوافق مع اسم الجدول الحالي
            modelBuilder.Entity<Parent>().ToTable("Parent");
        }
    }
}