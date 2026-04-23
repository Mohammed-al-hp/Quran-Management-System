using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    /// <summary>
    /// خدمة أولياء الأمور - تدير ملفات أولياء الأمور وعلاقاتهم بالطلاب
    /// </summary>
    public interface IParentService
    {
        Task<IEnumerable<Parent>> GetAllParentsAsync();
        Task<Parent?> GetParentByIdAsync(int id);
        Task<Parent?> GetParentByEmailAsync(string email);
        Task<IEnumerable<Student>> GetChildrenAsync(int parentId);
        Task<Student?> GetChildWithDetailsAsync(int parentId, int studentId);
        Task CreateParentAsync(Parent parent);
        Task UpdateParentAsync(Parent parent);
        Task DeleteParentAsync(int id);
    }
}
