using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Models;

namespace QuranCentersSystem.Data
{
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
        public DbSet<QuranCentersSystem.Models.Memorization> Memorizations { get; set; }
    }
}