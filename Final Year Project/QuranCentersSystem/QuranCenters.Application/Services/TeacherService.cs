using Microsoft.EntityFrameworkCore;
using QuranCenters.Application.Interfaces;
using QuranCenters.Core.Entities;
using QuranCenters.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuranCenters.Application.Services
{
    public class TeacherService : ITeacherService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TeacherService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Teacher>> GetAllTeachersAsync(string? searchTerm = null, string? sortBy = null)
        {
            var query = _unitOfWork.Repository<Teacher>().Query()
                .Include(t => t.Circles)
                    .ThenInclude(c => c.Students);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.Name.Contains(searchTerm) || t.Qualification.Contains(searchTerm));
            }

            query = sortBy switch
            {
                "oldest" => query.OrderBy(t => t.Id),
                "students" => query.OrderByDescending(t => t.Circles.SelectMany(c => c.Students).Count()),
                "alphabetical" => query.OrderBy(t => t.Name),
                _ => query.OrderByDescending(t => t.Id),
            };

            return await query.ToListAsync();
        }

        public async Task<Teacher?> GetTeacherByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Teacher>().GetByIdAsync(id);
        }

        public async Task<Teacher?> GetTeacherWithCirclesAsync(int id)
        {
            return await _unitOfWork.Repository<Teacher>().Query()
                .Include(t => t.Circles)
                    .ThenInclude(c => c.Students)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<Teacher?> GetTeacherByContactAsync(string emailOrPhone)
        {
            return await _unitOfWork.Repository<Teacher>().Query()
                .Include(t => t.Circles)
                    .ThenInclude(c => c.Students)
                .FirstOrDefaultAsync(t => t.Phone == emailOrPhone || t.Name.Contains(emailOrPhone));
        }

        public async Task CreateTeacherAsync(Teacher teacher, int? circleId = null)
        {
            await _unitOfWork.Repository<Teacher>().AddAsync(teacher);
            await _unitOfWork.SaveChangesAsync();

            if (circleId.HasValue)
            {
                var circle = await _unitOfWork.Repository<Circle>().GetByIdAsync(circleId.Value);
                if (circle != null)
                {
                    circle.TeacherId = teacher.Id;
                    _unitOfWork.Repository<Circle>().Update(circle);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateTeacherAsync(Teacher teacher)
        {
            _unitOfWork.Repository<Teacher>().Update(teacher);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteTeacherAsync(int id)
        {
            var teacher = await _unitOfWork.Repository<Teacher>().Query()
                .Include(t => t.Circles)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (teacher != null)
            {
                foreach (var circle in teacher.Circles)
                {
                    circle.TeacherId = null;
                }
                _unitOfWork.Repository<Teacher>().Delete(teacher);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<int> GetTeachersCountAsync()
        {
            return await _unitOfWork.Repository<Teacher>().Query().CountAsync();
        }
    }
}
