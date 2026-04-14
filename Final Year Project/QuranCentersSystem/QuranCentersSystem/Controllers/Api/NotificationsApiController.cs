using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QuranCentersSystem.Controllers.Api
{
    /// <summary>
    /// واجهة برمجة الإشعارات - جلب وإدارة إشعارات المستخدم
    /// محمية بمصادقة JWT
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NotificationsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// جلب إشعارات المستخدم الحالي
        /// GET: api/notifications?unreadOnly=true
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { success = false, message = "غير مصرح" });

            var query = _context.Notifications
                .Where(n => n.UserId == userId)
                .AsQueryable();

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Message,
                    n.Type,
                    n.IsRead,
                    createdAt = n.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    n.RelatedEntityId
                })
                .ToListAsync();

            var unreadCount = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();

            return Ok(new
            {
                notifications,
                unreadCount
            });
        }

        /// <summary>
        /// تحديد إشعار كمقروء
        /// POST: api/notifications/{id}/read
        /// </summary>
        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound(new { success = false, message = "الإشعار غير موجود" });

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "تم تحديد الإشعار كمقروء" });
        }

        /// <summary>
        /// تحديد جميع الإشعارات كمقروءة
        /// POST: api/notifications/read-all
        /// </summary>
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var unreadNotifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = $"تم تحديد {unreadNotifications.Count} إشعار كمقروء"
            });
        }
    }
}
