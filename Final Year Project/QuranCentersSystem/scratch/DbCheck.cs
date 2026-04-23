using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuranCenters.Infrastructure.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DbCheck
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=QuranCentersDB;Trusted_Connection=True;TrustServerCertificate=True;");

            using (var context = new ApplicationDbContext(optionsBuilder.Options))
            {
                var users = await context.Users.ToListAsync();
                Console.WriteLine($"Total Users: {users.Count}");
                foreach (var user in users)
                {
                    Console.WriteLine($"User: Email={user.Email}, NormalizedEmail={user.NormalizedEmail}, UserName={user.UserName}");
                }

                var roles = await context.Roles.ToListAsync();
                Console.WriteLine($"Total Roles: {roles.Count}");
                foreach (var role in roles)
                {
                    Console.WriteLine($"Role: Name={role.Name}, NormalizedName={role.NormalizedName}");
                }
            }
        }
    }
}
