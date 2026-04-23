using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    public class ParentService : IParentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ParentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Parent>> GetAllParentsAsync()
        {
            return await _unitOfWork.Repository<Parent>().Query()
                .Include(p => p.Students)
                .ToListAsync();
        }

        public async Task<Parent?> GetParentByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Parent>().GetByIdAsync(id);
        }

        public async Task<Parent?> GetParentByEmailAsync(string email)
        {
            return await _unitOfWork.Repository<Parent>().Query()
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<IEnumerable<Student>> GetChildrenAsync(int parentId)
        {
            return await _unitOfWork.Repository<Student>().Query()
                .Where(s => s.ParentId == parentId)
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .Include(s => s.Payments)
                .Include(s => s.PointsLedgers)
                .Include(s => s.StudentBadges)
                .ToListAsync();
        }

        public async Task<Student?> GetChildWithDetailsAsync(int parentId, int studentId)
        {
            return await _unitOfWork.Repository<Student>().Query()
                .Where(s => s.Id == studentId && s.ParentId == parentId)
                .Include(s => s.Parent)
                .Include(s => s.Circle)
                .Include(s => s.Attendances)
                .Include(s => s.Memorizations)
                .Include(s => s.Payments)
                .Include(s => s.PointsLedgers)
                .Include(s => s.StudentBadges)
                .FirstOrDefaultAsync();
        }

        public async Task CreateParentAsync(Parent parent)
        {
            await _unitOfWork.Repository<Parent>().AddAsync(parent);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateParentAsync(Parent parent)
        {
            _unitOfWork.Repository<Parent>().Update(parent);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteParentAsync(int id)
        {
            var parent = await _unitOfWork.Repository<Parent>().GetByIdAsync(id);
            if (parent != null)
            {
                _unitOfWork.Repository<Parent>().Delete(parent);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
