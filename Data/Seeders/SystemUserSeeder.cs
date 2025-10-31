using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BilginetAkademi.Models;
using System;
using System.Threading.Tasks;

namespace BilginetAkademi.Data.Seeders
{
    public class SystemUserSeeder : ISeeder
    {
        private readonly IServiceProvider _serviceProvider;

        public SystemUserSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Migration'ları uygula
            await dbContext.Database.MigrateAsync();

            // ID'si 1 olan kullanıcının varlığını kontrol et
            var systemUser = await userManager.FindByIdAsync("1");

            // Eğer sistem kullanıcısı yoksa oluştur
            if (systemUser == null)
            {
                systemUser = new User
                {
                    UserName = "system_service",
                    Email = "system@bilginetakademi.com",
                    FirstName = "System",
                    LastName = "Service",
                    EmailConfirmed = true,
                    // İlk kayıt sırasında 0 bırakılır
                    CreatedBy = 0,
                    UpdatedBy = 0,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Kullanıcı oluşturma
                var result = await userManager.CreateAsync(systemUser, "System!@#123");

                if (result.Succeeded)
                {
                    // KRİTİK DÜZELTME: Oluşturulan kullanıcının CreatedBy/UpdatedBy alanlarını kendi ID'sine ayarla
                    if (systemUser.Id > 0)
                    {
                        systemUser.CreatedBy = systemUser.Id;
                        systemUser.UpdatedBy = systemUser.Id;
                        await userManager.UpdateAsync(systemUser);
                    }
                }
            }
        }
    }
}