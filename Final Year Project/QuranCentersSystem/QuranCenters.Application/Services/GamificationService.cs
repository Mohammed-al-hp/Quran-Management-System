using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly IRepository<PointsLedger> _pointsRepo;
        private readonly IRepository<StudentBadge> _badgeRepo;
        private readonly INotificationService _notificationService;

        public GamificationService(
            IRepository<PointsLedger> pointsRepo, 
            IRepository<StudentBadge> badgeRepo,
            INotificationService notificationService)
        {
            _pointsRepo = pointsRepo;
            _badgeRepo = badgeRepo;
            _notificationService = notificationService;
        }

        public async Task AwardPointsAsync(int studentId, int points, string reason)
        {
            if (points == 0) return;

            var ledger = new PointsLedger
            {
                StudentId = studentId,
                Points = points,
                Reason = reason,
                DateAwarded = DateTime.Now
            };

            await _pointsRepo.AddAsync(ledger);
            await _pointsRepo.SaveChangesAsync();

            // Trigger real-time notification to parent
            await _notificationService.NotifyParentOnPointsAwarded(studentId, points, reason);

            await CheckAndAwardBadgesAsync(studentId);
        }

        public async Task ProcessDailyMemorizationPointsAsync(Memorization memorization)
        {
            int pointsToAward = 0;
            if (memorization.Grade == "ممتاز") // 5 Stars
            {
                pointsToAward = 50;
            }
            else if (memorization.Grade == "جيد جداً") // 4 Stars
            {
                pointsToAward = 40;
            }
            else if (memorization.Grade == "جيد") // 3 Stars
            {
                pointsToAward = 30;
            }

            if (pointsToAward > 0)
            {
                await AwardPointsAsync(memorization.StudentId, pointsToAward, "تسميع متميز: " + memorization.SurahName);
            }
        }

        public async Task ProcessAttendancePointsAsync(Attendance attendance)
        {
            if (attendance.Status == "حاضر" || attendance.Status == "Present")
            {
                await AwardPointsAsync(attendance.StudentId, 2, "حضور في الموعد المرجو");
            }
        }

        public async Task CheckAndAwardBadgesAsync(int studentId)
        {
            var pointsLedger = await _pointsRepo.GetAllAsync();
            var totalPoints = pointsLedger.Where(p => p.StudentId == studentId).Sum(p => p.Points);

            var pastBadges = await _badgeRepo.GetAllAsync();
            var earnedBadgeNames = pastBadges.Where(b => b.StudentId == studentId).Select(b => b.BadgeName).ToList();

            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 50, "مبتدئ", "/images/badges/bronze.png");
            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 150, "مثابر", "/images/badges/silver.png");
            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 300, "متميز", "/images/badges/gold.png");
            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 500, "متقن", "/images/badges/diamond.png");
        }

        private async Task CheckAndAwardSingleBadge(int studentId, int totalPoints, List<string> earned, int threshold, string badgeName, string iconUrl)
        {
            if (totalPoints >= threshold && !earned.Contains(badgeName))
            {
                var newBadge = new StudentBadge
                {
                    StudentId = studentId,
                    BadgeName = badgeName,
                    IconUrl = iconUrl,
                    DateEarned = DateTime.Now
                };
                await _badgeRepo.AddAsync(newBadge);
                await _badgeRepo.SaveChangesAsync();

                // Trigger real-time notification to parent
                await _notificationService.NotifyParentOnNewBadge(studentId, badgeName);
            }
        }
    }
}
