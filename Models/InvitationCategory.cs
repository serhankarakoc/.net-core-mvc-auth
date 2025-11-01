using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DavetLink.Models
{
    [Table("invitation_categories")]
    public class InvitationCategory : BaseModel
    {
        // Zorunlu Alanlar
        [Required]
        [StringLength(255)]
        public string Template { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Icon { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        // İlişki Tanımı
        public virtual ICollection<Invitation> Invitations { get; set; } = new List<Invitation>();
    }
}