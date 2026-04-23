using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    public class CircleService : ICircleService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CircleService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Circle>> GetAllCirclesAsync()
        {
            return await _unitOfWork.Repository<Circle>().Query()
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .ToListAsync();
        }

        public async Task<Circle?> GetCircleByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Circle>().GetByIdAsync(id);
        }

        public async Task<Circle?> GetCircleWithStudentsAsync(int id)
        {
            return await _unitOfWork.Repository<Circle>().Query()
                .Include(c => c.Teacher)
                .Include(c => c.Students)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task CreateCircleAsync(Circle circle)
        {
            await _unitOfWork.Repository<Circle>().AddAsync(circle);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateCircleAsync(Circle circle)
        {
            _unitOfWork.Repository<Circle>().Update(circle);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteCircleAsync(int id)
        {
            var circle = await _unitOfWork.Repository<Circle>().GetByIdAsync(id);
            if (circle != null)
            {
                _unitOfWork.Repository<Circle>().Delete(circle);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task AssignTeacherAsync(int circleId, int teacherId)
        {
            var circle = await _unitOfWork.Repository<Circle>().GetByIdAsync(circleId);
            if (circle != null)
            {
                circle.TeacherId = teacherId;
                _unitOfWork.Repository<Circle>().Update(circle);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<int> GetCirclesCountAsync()
        {
            return await _unitOfWork.Repository<Circle>().Query().CountAsync();
        }
    }
}
