using Microsoft.AspNetCore.Identity;

namespace BilginetAkademi.Models
{
    public class Role : IdentityRole<int>, IBaseEntity
    {
        // BaseEntity property'leri
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation property
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }

        public Role() : base() { }
        public Role(string roleName) : base(roleName) { }
    }
}