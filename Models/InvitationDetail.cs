using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DavetLink.Models
{
    [Table("invitation_details")]
    public class InvitationDetail : BaseModel
    {
        // Zorunlu alan (Birebir ilişki için)
        [Required]
        public uint InvitationID { get; set; }

        // Opsiyonel Alanlar
        [StringLength(255)]
        public string? Title { get; set; }

        [StringLength(255)]
        public string? Person { get; set; }

        // Kişinin Ebeveynleri
        [Required]
        public bool IsMotherLive { get; set; } = true;

        [StringLength(100)]
        public string? MotherName { get; set; }

        [StringLength(100)]
        public string? MotherSurname { get; set; }

        [Required]
        public bool IsFatherLive { get; set; } = true;

        [StringLength(100)]
        public string? FatherName { get; set; }

        [StringLength(100)]
        public string? FatherSurname { get; set; }

        // Gelin Detayları
        [StringLength(100)]
        public string? BrideName { get; set; }

        [StringLength(100)]
        public string? BrideSurname { get; set; }

        [Required]
        public bool IsBrideMotherLive { get; set; } = true;

        [StringLength(100)]
        public string? BrideMotherName { get; set; }

        [StringLength(100)]
        public string? BrideMotherSurname { get; set; }

        [Required]
        public bool IsBrideFatherLive { get; set; } = true;

        [StringLength(100)]
        public string? BrideFatherName { get; set; }

        [StringLength(100)]
        public string? BrideFatherSurname { get; set; }

        // Damat Detayları
        [StringLength(100)]
        public string? GroomName { get; set; }

        [StringLength(100)]
        public string? GroomSurname { get; set; }

        [Required]
        public bool IsGroomMotherLive { get; set; } = true;

        [StringLength(100)]
        public string? GroomMotherName { get; set; }

        [StringLength(100)]
        public string? GroomMotherSurname { get; set; }

        [Required]
        public bool IsGroomFatherLive { get; set; } = true;

        [StringLength(100)]
        public string? GroomFatherName { get; set; }

        [StringLength(100)]
        public string? GroomFatherSurname { get; set; }

        // İlişki Tanımı
        public virtual Invitation? Invitation { get; set; }
    }
}