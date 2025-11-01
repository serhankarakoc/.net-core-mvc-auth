using CoreApplication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace DavetLink.Data
{
    // IdentityDbContext<TUser, TRole, TKey> yapısını User, Role ve int PK ile kullanır
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor) // IHttpContextAccessor enjekte ediliyor
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Yeni entity'ler için DbSet'ler (ihtiyacınız olursa yorum satırını kaldırın)
        // public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // IdentityDbContext'in varsayılan yapılandırmasını çağırın (Identity tabloları için gereklidir)
            base.OnModelCreating(builder);

            // Tüm IBaseEntity uygulayan entity'ler için ortak yapılandırmalar
            ConfigureBaseEntityProperties<User>(builder);
            ConfigureBaseEntityProperties<Role>(builder);
            // ConfigureBaseEntityProperties<Product>(builder);

            // Entity-specific configurations (Özellikle kendi kendine referans veren ilişkiler)
            ConfigureUser(builder);
            ConfigureRole(builder);
            // ConfigureProduct(builder);
        }

        // IBaseEntity uygulayan tüm entity'ler için ortak yapılandırma
        private void ConfigureBaseEntityProperties<T>(ModelBuilder builder) where T : class, IBaseEntity
        {
            builder.Entity<T>(entity =>
            {
                // BaseEntity configuration
                entity.Property(e => e.CreatedAt)
                    .IsRequired()
                    // PostgreSQL için zaman damgası varsayılan değeri
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                    .IsRequired()
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                // Soft Delete Global Query Filter: DeletedAt alanı null olmayan kayıtları gizler
                entity.HasQueryFilter(e => e.DeletedAt == null);

                // Indexes
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UpdatedAt);
                entity.HasIndex(e => e.DeletedAt);
                entity.HasIndex(e => e.IsActive);
            });
        }

        // User modeli için özel yapılandırma (Audit İlişkileri)
        private void ConfigureUser(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                // CreatedBy -> User
                entity.HasOne(u => u.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // CreatedBy NULL olabilir

                // UpdatedBy -> User
                entity.HasOne(u => u.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // UpdatedBy NULL olabilir

                // DeletedBy -> User
                entity.HasOne(u => u.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // DeletedBy NULL olabilir
            });
        }

        // Role modeli için özel yapılandırma (Audit İlişkileri)
        private void ConfigureRole(ModelBuilder builder)
        {
            builder.Entity<Role>(entity =>
            {
                // CreatedBy -> User
                entity.HasOne(r => r.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // CreatedBy NULL olabilir

                // UpdatedBy -> User
                entity.HasOne(r => r.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // UpdatedBy NULL olabilir

                // DeletedBy -> User
                entity.HasOne(r => r.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false); // DeletedBy NULL olabilir
            });
        }

        // SaveChanges override for audit fields (Synchronous)
        public override int SaveChanges()
        {
            UpdateAuditFields();
            return base.SaveChanges();
        }

        // SaveChanges override for audit fields (Asynchronous)
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateAuditFields();
            return base.SaveChangesAsync(cancellationToken);
        }

        // Denetim (Audit) alanlarını otomatik olarak güncelleyen metod
        private void UpdateAuditFields()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is IBaseEntity)
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            // Servis/Seeder işlemleri için null (ID = 0) değerini alacak.
            var currentUserId = GetCurrentUserId();

            foreach (var entry in entries)
            {
                var entity = (IBaseEntity)entry.Entity;
                var now = DateTime.UtcNow;

                // Eğer currentUserId 0 ise (yani anonim veya seeder), audit alanlarına null atayın.
                // Aksi takdirde, geçerli kullanıcı ID'sini (currentUserId) atayın.
                int? auditId = currentUserId > 0 ? currentUserId : (int?)null;


                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                    entity.UpdatedAt = now;
                    // CreatedBy ve UpdatedBy alanlarına null veya gerçek ID atanır.
                    entity.CreatedBy = auditId;
                    entity.UpdatedBy = auditId;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = auditId;

                    var originalDeletedAt = entry.OriginalValues[nameof(IBaseEntity.DeletedAt)] as DateTime?;

                    // Soft Delete işlemi: DeletedAt null iken bir değere atanmışsa, DeletedBy'ı ayarla
                    if (entity.DeletedAt != null && originalDeletedAt == null)
                    {
                        entity.DeletedBy = auditId; // DeletedBy alanına null veya gerçek ID atanır.
                    }

                    // CreatedAt ve CreatedBy alanlarının manuel olarak güncellenmesini önle
                    if (entry.Property(nameof(IBaseEntity.CreatedAt)).IsModified)
                        entry.Property(nameof(IBaseEntity.CreatedAt)).IsModified = false;

                    if (entry.Property(nameof(IBaseEntity.CreatedBy)).IsModified)
                        entry.Property(nameof(IBaseEntity.CreatedBy)).IsModified = false;
                }
            }
        }

        // HTTP Bağlamından (Context) oturum açmış kullanıcının ID'sini döndürür
        private int GetCurrentUserId()
        {
            // Eğer HTTP bağlamı veya kullanıcı yoksa (örneğin arka plan görevi), 0 döndür
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                // ClaimTypes.NameIdentifier, Identity tarafından kullanılan User ID claim'idir
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
            }
            // Anonim kullanıcılar veya servis işlemleri için 0 döndürülür
            return 0;
        }
    }
}
