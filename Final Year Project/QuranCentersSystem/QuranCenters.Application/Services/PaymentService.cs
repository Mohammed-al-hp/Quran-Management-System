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
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _unitOfWork.Repository<Payment>().Query()
                .Include(p => p.Student)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStudentAsync(int studentId)
        {
            return await _unitOfWork.Repository<Payment>().Query()
                .Where(p => p.StudentId == studentId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int id)
        {
            return await _unitOfWork.Repository<Payment>().GetByIdAsync(id);
        }

        public async Task CreatePaymentAsync(Payment payment, string createdBy)
        {
            payment.CreatedBy = createdBy;
            payment.PaymentDate = payment.PaymentDate == default ? DateTime.Now : payment.PaymentDate;
            await _unitOfWork.Repository<Payment>().AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _unitOfWork.Repository<Payment>().Update(payment);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeletePaymentAsync(int id)
        {
            var payment = await _unitOfWork.Repository<Payment>().GetByIdAsync(id);
            if (payment != null)
            {
                _unitOfWork.Repository<Payment>().Delete(payment);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<decimal> GetTotalPaymentsAsync(int? studentId = null)
        {
            var query = _unitOfWork.Repository<Payment>().Query();
            if (studentId.HasValue)
            {
                query = query.Where(p => p.StudentId == studentId.Value);
            }
            return await query.SumAsync(p => p.Amount);
        }
    }
}
