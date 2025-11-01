using System.ComponentModel.DataAnnotations;

namespace DavetLink.Models
{
    public abstract class BaseModel : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeletedBy { get; set; }
        public bool IsActive { get; set; } = true;
    }
}