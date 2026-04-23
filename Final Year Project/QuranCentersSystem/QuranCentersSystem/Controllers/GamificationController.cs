using Microsoft.AspNetCore.Mvc;
using QuranCenters.Application.Interfaces;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers
{
    public class GamificationController : Controller
    {
        private readonly IGamificationService _gamificationService;

        public GamificationController(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        public async Task<IActionResult> Leaderboard()
        {
            var viewModel = await _gamificationService.GetLeaderboardAsync();
            return View(viewModel);
        }
    }
}
