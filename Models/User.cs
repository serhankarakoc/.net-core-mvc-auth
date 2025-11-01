using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CoreApplication.Models
{
    public class User : IdentityUser<int>, IBaseEntity
    {
        // BaseEntity property'leri
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public bool IsActive { get; set; } = true;

        // Custom property'ler
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        public DateTime? DateOfBirth { get; set; }

        // Navigation property
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        public virtual User? DeletedByUser { get; set; }

        public User() : base() { }
        public User(string userName) : base(userName) { }
    }

}
