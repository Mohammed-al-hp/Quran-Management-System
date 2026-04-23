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

        // --- الجداول الأساسية ---
        public DbSet<Student> Students { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Circle> Circles { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Memorization> Memorizations { get; set; }
        public DbSet<MemorizationQuestion> MemorizationQuestions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        // --- جداول التلعيب ---
        public DbSet<PointsLedger> PointsLedgers { get; set; }
        public DbSet<StudentBadge> StudentBadges { get; set; }

        // --- الجداول الجديدة (Checkpoint 1) ---
        public DbSet<Message> Messages { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<KhatmProgress> KhatmProgresses { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }

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

            // --- علاقات الجداول الجديدة ---

            // Assignment → Circle
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Circle)
                .WithMany()
                .HasForeignKey(a => a.CircleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignment → Teacher (optional)
            modelBuilder.Entity<Assignment>()
                .HasOne(a => a.Teacher)
                .WithMany()
                .HasForeignKey(a => a.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // AssignmentSubmission → Assignment
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Assignment)
                .WithMany(a => a.Submissions)
                .HasForeignKey(s => s.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // AssignmentSubmission → Student
            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(s => s.Student)
                .WithMany(st => st.AssignmentSubmissions)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // KhatmProgress → Student
            modelBuilder.Entity<KhatmProgress>()
                .HasOne(k => k.Student)
                .WithMany(s => s.KhatmProgresses)
                .HasForeignKey(k => k.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KhatmProgress>()
                .Property(k => k.TotalPagesMemorized)
                .HasPrecision(7, 2);

            // Message → Student (optional relation)
            modelBuilder.Entity<Message>()
                .HasOne(m => m.RelatedStudent)
                .WithMany()
                .HasForeignKey(m => m.RelatedStudentId)
                .OnDelete(DeleteBehavior.SetNull);

            // SystemSetting - unique key constraint
            modelBuilder.Entity<SystemSetting>()
                .HasIndex(s => s.Key)
                .IsUnique();
        }
    }
}