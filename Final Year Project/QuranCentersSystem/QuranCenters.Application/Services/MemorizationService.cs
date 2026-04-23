using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    public class MemorizationService : IMemorizationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGamificationService _gamificationService;

        public MemorizationService(IUnitOfWork unitOfWork, IGamificationService gamificationService)
        {
            _unitOfWork = unitOfWork;
            _gamificationService = gamificationService;
        }

        public async Task<Memorization?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Memorization>().GetByIdAsync(id);
        }

        public async Task<IEnumerable<Memorization>> GetRecentRecordsAsync(int count = 10)
        {
            return await _unitOfWork.Repository<Memorization>().Query()
                .Include(m => m.Student)
                .OrderByDescending(m => m.Date)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Memorization>> GetStudentRecordsAsync(int studentId)
        {
            return await _unitOfWork.Repository<Memorization>().Query()
                .Where(m => m.StudentId == studentId)
                .Include(m => m.Student)
                .OrderByDescending(m => m.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Memorization>> GetTodayRecordsAsync()
        {
            var today = DateTime.Today;
            return await _unitOfWork.Repository<Memorization>().Query()
                .Include(m => m.Student)
                .Where(m => m.Date >= today)
                .OrderByDescending(m => m.Id)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IEnumerable<Memorization>> GetRecordsByCircleAsync(int circleId)
        {
            return await _unitOfWork.Repository<Memorization>().Query()
                .Include(m => m.Student)
                .Where(m => m.Student.CircleId == circleId)
                .OrderByDescending(m => m.Date)
                .ToListAsync();
        }

        public async Task CreateRecordAsync(Memorization memorization)
        {
            await _unitOfWork.Repository<Memorization>().AddAsync(memorization);
            await _unitOfWork.SaveChangesAsync();

            // Process gamification points
            await _gamificationService.ProcessDailyMemorizationPointsAsync(memorization);
        }

        public async Task CreateRecordWithQuestionsAsync(Memorization memorization, string[] questions, string[] answers)
        {
            await _unitOfWork.Repository<Memorization>().AddAsync(memorization);
            await _unitOfWork.SaveChangesAsync();

            if (questions != null)
            {
                for (int i = 0; i < questions.Length; i++)
                {
                    if (!string.IsNullOrEmpty(questions[i]))
                    {
                        var q = new MemorizationQuestion
                        {
                            MemorizationId = memorization.Id,
                            QuestionText = questions[i],
                            StudentAnswer = answers.Length > i ? answers[i] : ""
                        };
                        await _unitOfWork.Repository<MemorizationQuestion>().AddAsync(q);
                    }
                }
                await _unitOfWork.SaveChangesAsync();
            }

            await _gamificationService.ProcessDailyMemorizationPointsAsync(memorization);
        }

        public async Task<int> GetTodayActiveStudentsCountAsync(List<int>? circleIds = null)
        {
            var query = _unitOfWork.Repository<Memorization>().Query()
                .Where(m => m.Date.Date == DateTime.Today);

            if (circleIds != null && circleIds.Count > 0)
            {
                query = query.Where(m => circleIds.Contains(m.Student.CircleId));
            }

            return await query.Select(m => m.StudentId).Distinct().CountAsync();
        }

        public async Task<GradeDistribution> GetGradeDistributionAsync()
        {
            var memorizations = _unitOfWork.Repository<Memorization>().Query();
            return new GradeDistribution
            {
                ExcellentCount = await memorizations.CountAsync(m => m.Grade == "ممتاز"),
                VeryGoodCount = await memorizations.CountAsync(m => m.Grade == "جيد جداً"),
                GoodCount = await memorizations.CountAsync(m => m.Grade == "جيد"),
                FairCount = await memorizations.CountAsync(m => m.Grade == "مقبول")
            };
        }
    }
}
