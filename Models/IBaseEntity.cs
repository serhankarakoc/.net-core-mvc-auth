namespace BilginetAkademi.Models
{
    public interface IBaseEntity
    {
        int Id { get; set; }
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        DateTime? DeletedAt { get; set; }
        int? CreatedBy { get; set; }
        int? UpdatedBy { get; set; }
        int? DeletedBy { get; set; }
        bool IsActive { get; set; }
    }
}