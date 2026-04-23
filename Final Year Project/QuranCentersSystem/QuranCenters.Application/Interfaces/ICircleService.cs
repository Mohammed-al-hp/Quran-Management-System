using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface ICircleService
    {
        Task<IEnumerable<Circle>> GetAllCirclesAsync();
        Task<Circle?> GetCircleByIdAsync(int id);
        Task<Circle?> GetCircleWithStudentsAsync(int id);
        Task CreateCircleAsync(Circle circle);
        Task UpdateCircleAsync(Circle circle);
        Task DeleteCircleAsync(int id);
        Task AssignTeacherAsync(int circleId, int teacherId);
        Task<int> GetCirclesCountAsync();
    }
}
