using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;

namespace QuranCenters.Infrastructure.Data
{
    /// <summary>
    /// سياق قاعدة البيانات الرئيسي - يدير جميع الجداول وعلاقاتها
    /// يرث من IdentityDbContext لدعم نظام الهوية والمصادقة مع ApplicationUser
    /// </summary>
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
        public DbSet<Memorization> Memorizations { get; set; }
        public DbSet<MemorizationQuestion> MemorizationQuestions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<PointsLedger> PointsLedgers { get; set; }
        public DbSet<StudentBadge> StudentBadges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // إعداد الترميز العربي لقاعدة البيانات بالكامل
            modelBuilder.UseCollation("Arabic_CI_AS");

            // تكوين جدول Parent للحفاظ على التوافق مع اسم الجدول الحالي
            modelBuilder.Entity<Parent>().ToTable("Parent");

            modelBuilder.Entity<Memorization>()
                .Property(m => m.PagesCount)
                .HasPrecision(5, 2);

            // Configure Student-Circle relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Circle)
                .WithMany(c => c.Students)
                .HasForeignKey(s => s.CircleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Student-Parent relationship
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Parent)
                .WithMany(p => p.Students)
                .HasForeignKey(s => s.ParentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Circle-Teacher relationship
            modelBuilder.Entity<Circle>()
                .HasOne(c => c.Teacher)
                .WithMany(t => t.Circles)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}