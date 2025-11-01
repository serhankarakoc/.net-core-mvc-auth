using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DavetLink.Models
{
    [Table("invitation_participants")]
    public class InvitationParticipant : BaseModel
    {
        // Zorunlu Alanlar
        [Required]
        public uint InvitationID { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Telephone { get; set; } = string.Empty;

        [Required]
        public int ParticipantCount { get; set; } = 1;

        // İlişki Tanımı
        public virtual Invitation Invitation { get; set; } = null!;
    }
}