using Microsoft.AspNetCore.Identity;
using BilginetAkademi.Models;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace BilginetAkademi.Data.Seeders
{
    public class RoleSeeder : ISeeder
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string[] _roleNames = { "Admin", "Eğitmen", "Öğrenci" };

        // Varsayım: SystemUser (ID=1) ilk oluşturulmuş kullanıcıdır.
        private const int SystemUserId = 1;

        public RoleSeeder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task SeedAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            foreach (var roleName in _roleNames)
            {
                // Rol yoksa oluştur
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var role = new Role(roleName)
                    {
                        // Audit alanlarını SystemUser ID'sine referans vererek doldur.
                        CreatedBy = SystemUserId,
                        UpdatedBy = SystemUserId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsActive = true
                    };
                    await roleManager.CreateAsync(role);
                }
            }
        }
    }
}