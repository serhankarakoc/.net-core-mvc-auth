using DavetLink.Data;
using DavetLink.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // 👈 Yeni Using: Identity Store'ları için gerekli olabilir
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 👇 BURASI EKLENDİ: HttpContextAccessor'ı DI konteynırına ekleyin
// ApplicationDbContext'teki SaveChanges metodu ile aktif kullanıcı ID'sini almak için zorunludur.
builder.Services.AddHttpContextAccessor();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
  options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// 👇 BURASI GÜNCELLENDİ: User yerine User ve Rol servisi eklendi.
// AddDefaultIdentity yerine AddIdentity kullanıldı.
builder.Services.AddIdentity<User, Role>(options =>
{
    // Identity seçeneklerinizi burada ayarlayın
    options.SignIn.RequireConfirmedAccount = true;
    // options.Password, options.Lockout, vb. ayarları buraya eklenebilir.
})
// ApplicationDbContext'i Identity Store olarak kullan
    .AddEntityFrameworkStores<ApplicationDbContext>()
    // UserStore ve RoleStore'u int PK tipiyle açıkça belirtmek daha güvenlidir.
    .AddUserStore<UserStore<User, Role, ApplicationDbContext, int>>()
    .AddRoleStore<RoleStore<Role, ApplicationDbContext, int>>()
    .AddDefaultTokenProviders() // Token desteği (şifre sıfırlama vb.)
    .AddDefaultUI(); // Varsayılan Identity Razor Pages için gerekli

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseRouting();

// ⚠️ DİKKAT: Authentication, Authorization'dan ÖNCE GELMELİDİR!
// Identity kullanırken kimlik doğrulama her zaman yetkilendirmeden önce çalışmalıdır.
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}")
  .WithStaticAssets();

app.MapRazorPages()
 .WithStaticAssets();

app.Run();
