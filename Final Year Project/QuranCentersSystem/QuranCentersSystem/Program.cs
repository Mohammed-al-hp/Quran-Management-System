using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuranCenters.Infrastructure.Data;
using QuranCentersSystem.Models;
using QuranCenters.Core.Entities;
using QuranCenters.Infrastructure.Identity;
using QuranCentersSystem.Middleware;
using QuranCenters.Application.Services;
using QuranCenters.Infrastructure.Services;
using System.Text;
using System.Text.Json.Serialization;
using Rotativa.AspNetCore;
<<<<<<< HEAD
using QuranCenters.Core.Interfaces;
using QuranCenters.Application.Interfaces;
using QuranCenters.Infrastructure.Repositories;
using QuranCenters.Infrastructure.Hubs;
=======
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad

var builder = WebApplication.CreateBuilder(args);

// --- 🌟 تكوين الترميز لدعم اللغة العربية بشكل كامل ---
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// 🌟 ضمان دعم النصوص العربية في الاستجابة
builder.Services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options => {
    // Add any global filters if needed
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.EnableSensitiveDataLogging();
});

<<<<<<< HEAD
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
=======
// --- 2. إعداد نظام الهوية لاستخدام ApplicationUser ---
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

<<<<<<< HEAD
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "ItqanQuranSystem2026SecureKeyMustBe32CharsOrMore!!";

builder.Services.AddAuthentication(options =>
{
=======
// --- 3. إعدادات JWT Authentication لـ Flutter ---
// نجلب القيم من ملف appsettings.json الذي أعددناه
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? "YourSuperSecretKey12345_MustBeLong");

// --- تعديل نظام Authentication ليدعم الويب والموبايل معاً ---
builder.Services.AddAuthentication(options =>
{
    // نجعل الكوكيز هي الافتراضية للمتصفح (MVC)
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
<<<<<<< HEAD
=======
    // إعدادات الـ JWT الخاصة بـ Flutter (تبقى كما هي)
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
<<<<<<< HEAD
        ValidIssuer = jwtSettings["Issuer"] ?? "ItqanQuranSystem",
        ValidAudience = jwtSettings["Audience"] ?? "ItqanFlutterApp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

=======
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// --- 4. إعداد سياسة CORS لـ Flutter ---
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRazorPages();

<<<<<<< HEAD
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Itqan Quran Management API", Version = "v1" });
});

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IQrCodeService, QrCodeService>();
builder.Services.AddScoped<IPdfReportService, PdfReportService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IGamificationService, GamificationService>();
builder.Services.AddSignalR();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
=======
var app = builder.Build();

// --- 5. إعدادات Middleware ---
if (!app.Environment.IsDevelopment())
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

<<<<<<< HEAD
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Itqan API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
=======
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// تفعيل CORS قبل الـ Authentication
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
app.UseCors("AllowFlutter");
app.UseAuthentication();
app.UseAuthorization();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();
app.MapHub<NotificationHub>("/notificationHub");

<<<<<<< HEAD
=======
// --- 6. بذر البيانات (Seed Data) ---
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

<<<<<<< HEAD
    string[] roleNames = { "Admin", "Teacher", "Student", "Parent" };
=======
    string[] roleNames = { "Admin", "Teacher", "Parent" };
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminEmail = "admin@quransystems.com";
<<<<<<< HEAD
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
=======
    var existingUser = await userManager.FindByEmailAsync(adminEmail);

    if (existingUser == null)
>>>>>>> 7097dff658495d1d8b18b5d9bb1a3b0e942784ad
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Role = "Admin",
            IsActive = true
        };

        var result = await userManager.CreateAsync(admin, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}

app.Run();