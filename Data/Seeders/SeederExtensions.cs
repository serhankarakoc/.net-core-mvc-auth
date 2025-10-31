using BilginetAkademi.Data.Seeders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BilginetAkademi.Data.Seeders
{
    public static class SeederExtensions
    {
        // Program.cs'te builder.Services üzerinde çağrılır.
        public static void AddSeeders(this IServiceCollection services)
        {
            // Tüm ISeeder sınıflarını kaydet.
            services.AddTransient<ISeeder, SystemUserSeeder>();
            services.AddTransient<ISeeder, RoleSeeder>();
        }

        // Program.cs'te app üzerinde çağrılır ve tüm seeder'ları çalıştırır.
        public static async Task SeedDatabaseAsync(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<IHost>>();

            // Çalıştırılacak tüm seeder tiplerini sırayla tanımlayın (SystemUser önce gelmeli)
            var seederTypes = new List<Type>
            {
                typeof(SystemUserSeeder),
                typeof(RoleSeeder)
            };

            foreach (var seederType in seederTypes)
            {
                try
                {
                    var seeder = (ISeeder)services.GetRequiredService(seederType);

                    logger.LogInformation($"Seeding işlemi başlatılıyor: {seederType.Name}");
                    await seeder.SeedAsync();
                    logger.LogInformation($"Seeding işlemi tamamlandı: {seederType.Name}");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Seeding işlemi sırasında kritik hata oluştu: {seederType.Name}");
                    // Hata oluşursa uygulamanın başlatılmasını engelleyebilirsiniz
                    // throw; 
                }
            }
        }
    }
}