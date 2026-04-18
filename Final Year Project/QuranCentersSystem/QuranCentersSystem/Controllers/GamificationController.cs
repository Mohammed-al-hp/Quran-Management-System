using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.DTOs;
using QuranCenters.Infrastructure.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    public class GamificationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GamificationController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Leaderboard()
        {
            var students = await _context.Students
                .Include(s => s.Circle)
                .Include(s => s.PointsLedgers)
                .Include(s => s.StudentBadges)
                .ToListAsync();

            var rankings = students.Select(s => new StudentRankDto
            {
                StudentId = s.Id,
                StudentName = s.Name,
                CircleName = s.Circle?.Name ?? "غير محدد",
                TotalPoints = s.PointsLedgers.Sum(p => p.Points),
                BadgeCount = s.StudentBadges.Count
            })
            .OrderByDescending(r => r.TotalPoints)
            .ToList();

            // Assign rank numbers
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
            }

            var viewModel = new LeaderboardViewModel
            {
                Rankings = rankings
            };

            return View(viewModel);
        }
    }
}
