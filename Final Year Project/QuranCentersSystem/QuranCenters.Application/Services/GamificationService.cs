using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.DTOs;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    /// <summary>
    /// خدمة التلعيب - تدير النقاط والأوسمة ولوحة المتصدرين
    /// تم إصلاح استخدام IUnitOfWork بدلاً من IRepository مباشرة
    /// </summary>
    public class GamificationService : IGamificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationService _notificationService;

        public GamificationService(
            IUnitOfWork unitOfWork,
            INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
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

            await _unitOfWork.Repository<PointsLedger>().AddAsync(ledger);
            await _unitOfWork.SaveChangesAsync();

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
            var totalPoints = await _unitOfWork.Repository<PointsLedger>().Query()
                .Where(p => p.StudentId == studentId)
                .SumAsync(p => p.Points);

            var earnedBadgeNames = await _unitOfWork.Repository<StudentBadge>().Query()
                .Where(b => b.StudentId == studentId)
                .Select(b => b.BadgeName)
                .ToListAsync();

            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 50, "مبتدئ", "/images/badges/bronze.png");
            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 150, "مثابر", "/images/badges/silver.png");
            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 300, "متميز", "/images/badges/gold.png");
            await CheckAndAwardSingleBadge(studentId, totalPoints, earnedBadgeNames, 500, "متقن", "/images/badges/diamond.png");
        }

        public async Task<LeaderboardViewModel> GetLeaderboardAsync()
        {
            var students = await _unitOfWork.Repository<Student>().Query()
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

            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].Rank = i + 1;
            }

            return new LeaderboardViewModel { Rankings = rankings };
        }

        public async Task<int> GetStudentTotalPointsAsync(int studentId)
        {
            return await _unitOfWork.Repository<PointsLedger>().Query()
                .Where(p => p.StudentId == studentId)
                .SumAsync(p => p.Points);
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
                await _unitOfWork.Repository<StudentBadge>().AddAsync(newBadge);
                await _unitOfWork.SaveChangesAsync();

                // Trigger real-time notification to parent
                await _notificationService.NotifyParentOnNewBadge(studentId, badgeName);
            }
        }
    }
}
