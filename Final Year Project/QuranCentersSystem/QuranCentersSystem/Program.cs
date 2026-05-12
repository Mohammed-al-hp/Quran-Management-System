using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuranCentersSystem.Data;
using QuranCentersSystem.Models;
using System.Text.Json.Serialization;
using Rotativa.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. إعدادات قاعدة البيانات ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. إعداد نظام الهوية لاستخدام ApplicationUser ---
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
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

// --- 3. إعدادات JWT Authentication لـ Flutter ---
var jwtSettings = builder.Configuration.GetSection("Jwt");
var keyString = jwtSettings["Key"] ?? "YourSuperSecretKey12345_MustBeLong";
var key = Encoding.UTF8.GetBytes(keyString);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// --- 4. إعداد سياسة CORS لـ Flutter ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutter", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddRazorPages();

builder.Services.AddSingleton<System.Text.Encodings.Web.HtmlEncoder>(
    System.Text.Encodings.Web.HtmlEncoder.Create(allowedRanges: new[] {
        System.Text.Unicode.UnicodeRanges.All
    }));

var app = builder.Build();

// --- 5. إعدادات Middleware ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("AllowFlutter");
app.UseAuthentication();
app.UseAuthorization();

RotativaConfiguration.Setup(app.Environment.WebRootPath, "Rotativa");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// --- 6. بذر البيانات (Seed Data) المصحح ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // ✅ التعديل هنا: أضفنا "Student" و "Supervisor" لضمان عدم حدوث خطأ عند الربط
    string[] roleNames = { "Admin", "Teacher", "Parent", "Student", "Supervisor" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }

    var adminEmail = "admin@quransystems.com";
    var existingUser = await userManager.FindByEmailAsync(adminEmail);

    if (existingUser == null)
    {
        var admin = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            Role = "Admin",
            IsActive = true
        };

        try
        {
            var result = await userManager.CreateAsync(admin, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Seed Data Error]: {ex.Message}");
        }
    }
}
app.MapControllers();
// إضافة ولي أمر تجريبي
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    // إنشاء حساب ولي الأمر
    var parentEmail = "parent@test.com";
    var existingParent = await userManager.FindByEmailAsync(parentEmail);
    if (existingParent == null)
    {
        var parentUser = new ApplicationUser
        {
            UserName = parentEmail,
            Email = parentEmail,
            EmailConfirmed = true,
            IsActive = true,
            Role = "Parent"
        };
        var result = await userManager.CreateAsync(parentUser, "Parent@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(parentUser, "Parent");

            // إضافة بيانات ولي الأمر
            var parent = new QuranCentersSystem.Models.Parent
            {
                Name = "والد علي احمد",
                Phone = "0501234567",
                Email = parentEmail
            };
            context.Add(parent);
            await context.SaveChangesAsync();

            // ربط الطالب بولي الأمر
            var student = await context.Students.FirstOrDefaultAsync(s => s.Username == "student_2026_1473");
            if (student != null)
            {
                student.ParentId = parent.Id;
                await context.SaveChangesAsync();
            }
        }
    }
}
app.Run();