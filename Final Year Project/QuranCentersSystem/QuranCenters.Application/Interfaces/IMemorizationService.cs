using QuranCenters.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface IMemorizationService
    {
        Task<Memorization?> GetByIdAsync(int id);
        Task<IEnumerable<Memorization>> GetRecentRecordsAsync(int count = 10);
        Task<IEnumerable<Memorization>> GetStudentRecordsAsync(int studentId);
        Task<IEnumerable<Memorization>> GetTodayRecordsAsync();
        Task<IEnumerable<Memorization>> GetRecordsByCircleAsync(int circleId);
        Task CreateRecordAsync(Memorization memorization);
        Task CreateRecordWithQuestionsAsync(Memorization memorization, string[] questions, string[] answers);
        Task<int> GetTodayActiveStudentsCountAsync(List<int>? circleIds = null);
        Task<GradeDistribution> GetGradeDistributionAsync();
    }

    public class GradeDistribution
    {
        public int ExcellentCount { get; set; }
        public int VeryGoodCount { get; set; }
        public int GoodCount { get; set; }
        public int FairCount { get; set; }
    }
}
