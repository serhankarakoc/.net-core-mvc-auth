namespace BilginetAkademi.Data.Seeders
{
    // Tüm seeder sınıflarının uygulayacağı arayüz
    public interface ISeeder
    {
        // Seeding işlemini asenkron olarak gerçekleştirir
        Task SeedAsync();
    }
}