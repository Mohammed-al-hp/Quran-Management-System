using QuranCenters.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace QuranCenters.Application.Interfaces
{
    /// <summary>
    /// خدمة المدفوعات - تدير جميع العمليات المالية للطلاب
    /// </summary>
    public interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByStudentAsync(int studentId);
        Task<Payment?> GetPaymentByIdAsync(int id);
        Task CreatePaymentAsync(Payment payment, string createdBy);
        Task UpdatePaymentAsync(Payment payment);
        Task DeletePaymentAsync(int id);
        Task<decimal> GetTotalPaymentsAsync(int? studentId = null);
    }
}
