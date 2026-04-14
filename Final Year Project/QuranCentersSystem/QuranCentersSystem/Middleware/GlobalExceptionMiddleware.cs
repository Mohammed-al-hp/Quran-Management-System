using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace QuranCentersSystem.Middleware
{
    /// <summary>
    /// معالج الأخطاء العام - يلتقط جميع الاستثناءات غير المعالجة
    /// يعرض رسائل خطأ ودية للمستخدم ويسجل التفاصيل التقنية في السجلات
    /// يميز بين طلبات API (يُرجع JSON) وطلبات MVC (يُعيد التوجيه لصفحة الخطأ)
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger,
            IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "حدث خطأ غير متوقع أثناء معالجة الطلب: {Path}", context.Request.Path);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = exception switch
            {
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = statusCode;

            // التمييز بين طلبات API وطلبات MVC
            bool isApiRequest = context.Request.Path.StartsWithSegments("/api") ||
                                context.Request.Headers["Accept"].ToString().Contains("application/json");

            if (isApiRequest)
            {
                var response = new
                {
                    success = false,
                    message = GetUserFriendlyMessage(statusCode),
                    details = _env.IsDevelopment() ? exception.Message : null,
                    traceId = context.TraceIdentifier
                };

                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                await context.Response.WriteAsync(jsonResponse);
            }
            else
            {
                // لطلبات MVC - إعادة التوجيه لصفحة الخطأ
                context.Response.Redirect("/Home/Error");
            }
        }

        /// <summary>
        /// ترجمة رمز الحالة لرسالة ودية بالعربية
        /// </summary>
        private static string GetUserFriendlyMessage(int statusCode)
        {
            return statusCode switch
            {
                401 => "غير مصرح لك بالوصول. يرجى تسجيل الدخول",
                403 => "ليس لديك صلاحية للوصول لهذا المحتوى",
                404 => "المحتوى المطلوب غير موجود",
                400 => "البيانات المرسلة غير صالحة",
                409 => "حدث تعارض في البيانات",
                _ => "حدث خطأ غير متوقع. يرجى المحاولة لاحقاً"
            };
        }
    }
}
