using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    public interface ITeacherService
    {
        Task<IEnumerable<Teacher>> GetAllTeachersAsync(string? searchTerm = null, string? sortBy = null);
        Task<Teacher?> GetTeacherByIdAsync(int id);
        Task<Teacher?> GetTeacherWithCirclesAsync(int id);
        Task<Teacher?> GetTeacherByContactAsync(string emailOrPhone);
        Task CreateTeacherAsync(Teacher teacher, int? circleId = null);
        Task UpdateTeacherAsync(Teacher teacher);
        Task DeleteTeacherAsync(int id);
        Task<int> GetTeachersCountAsync();
    }
}
