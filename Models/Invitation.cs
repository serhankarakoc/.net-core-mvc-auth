using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DavetLink.Models
{
    [Table("invitations")]
    public class Invitation : BaseModel
    {
        [Required]
        [StringLength(100)]
        public string InvitationKey { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Image { get; set; } = string.Empty;

        [Required]
        public uint UserID { get; set; }

        [Required]
        public uint CategoryID { get; set; }

        [Required]
        public bool IsConfirmed { get; set; } = false;

        [Required]
        public bool IsParticipant { get; set; } = false;

        [Required]
        public bool IsMultipleParticipant { get; set; } = false;

        [Required]
        public bool IsFree { get; set; }

        public string? Description { get; set; }

        [StringLength(255)]
        public string? Venue { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        [StringLength(255)]
        public string? Link { get; set; }

        [StringLength(20)]
        public string? Telephone { get; set; }

        public string? Note { get; set; }

        public DateTime Date { get; set; }

        [StringLength(10)]
        public string? Time { get; set; }

        // Navigation Properties
        public virtual User? User { get; set; }

        public virtual InvitationCategory? Category { get; set; }

        public virtual InvitationDetail? InvitationDetail { get; set; }

        public virtual ICollection<InvitationParticipant> Participants { get; set; } = new List<InvitationParticipant>();
    }
}