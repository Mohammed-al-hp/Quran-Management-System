using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System.Text.Json.Serialization;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// --- 1. إعدادات قاعدة البيانات ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. إعداد نظام الهوية (تعديل ليتناسب مع جداولك) ---
// قمت بتغيير IdentityUser إلى ApplicationUser إذا كنت تستخدم كلاس مخصص كما ظهر في قاعدة بياناتك
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false; // تسهيل لعملية التطوير
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// --- 3. إعداد سياسة CORS (تجميع السياسات في سياسة واحدة قوية) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// إضافة التحكم في الـ JSON
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRazorPages();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    options.SlidingExpiration = true;
});

var app = builder.Build();

// --- 4. إعدادات Middleware ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// الترتيب هنا مهم جداً لعمل الـ API بشكل صحيح
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// تفعيل CORS بعد الـ Routing وقبل الـ Auth
app.UseCors("AllowFlutter");

app.UseAuthentication();
app.UseAuthorization();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// --- 5. بذر البيانات (Seed Data) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    // التأكد من الأدوار
    string[] roleNames = { "Admin", "Teacher", "Parent" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    // إنشاء حساب المدير الافتراضي
    var adminEmail = "admin@itqan.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(user, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

app.Run();