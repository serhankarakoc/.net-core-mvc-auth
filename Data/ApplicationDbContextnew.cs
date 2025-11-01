using DavetLink.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace DavetLink.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Ortak yapılandırmalar
            ConfigureBaseEntityProperties<User>(builder);
            ConfigureBaseEntityProperties<Role>(builder);

            // Özelleştirilmiş audit ilişkileri
            ConfigureUser(builder);
            ConfigureRole(builder);
        }

        private void ConfigureBaseEntityProperties<T>(ModelBuilder builder) where T : class, IBaseEntity
        {
            builder.Entity<T>(entity =>
            {
                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.UpdatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.IsActive)
                      .HasDefaultValue(true);

                entity.HasQueryFilter(e => e.DeletedAt == null);

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UpdatedAt);
                entity.HasIndex(e => e.DeletedAt);
                entity.HasIndex(e => e.IsActive);
            });
        }

        private void ConfigureUser(ModelBuilder builder)
        {
            builder.Entity<User>(entity =>
            {
                entity.HasOne(u => u.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(u => u.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(u => u.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(u => u.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
        }

        private void ConfigureRole(ModelBuilder builder)
        {
            builder.Entity<Role>(entity =>
            {
                entity.HasOne(r => r.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.CreatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(r => r.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.UpdatedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                entity.HasOne(r => r.DeletedByUser)
                    .WithMany()
                    .HasForeignKey(r => r.DeletedBy)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);
            });
        }

        // SaveChanges override
        public override int SaveChanges()
        {
            ApplyAuditInfo();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyAuditInfo();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyAuditInfo()
        {
            var currentUserId = GetCurrentUserId();
            int? auditId = currentUserId > 0 ? currentUserId : (int?)null;
            var now = DateTime.Now;

            foreach (var entry in ChangeTracker.Entries<IBaseEntity>())
            {
                var entity = entry.Entity;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entity.CreatedAt = now;
                        entity.UpdatedAt = now;
                        entity.CreatedBy = auditId;
                        entity.UpdatedBy = auditId;
                        break;

                    case EntityState.Modified:
                        entity.UpdatedAt = now;
                        entity.UpdatedBy = auditId;

                        // Eğer DeletedAt yeni atanmışsa (soft delete)
                        if (entity.DeletedAt != null && entry.OriginalValues[nameof(IBaseEntity.DeletedAt)] == null)
                            entity.DeletedBy = auditId;

                        // Created alanlarını koru
                        entry.Property(nameof(IBaseEntity.CreatedAt)).IsModified = false;
                        entry.Property(nameof(IBaseEntity.CreatedBy)).IsModified = false;
                        break;
                }
            }
        }

        private int GetCurrentUserId()
        {
            if (_httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim, out var userId))
                    return userId;
            }
            return 0;
        }
    }
}
