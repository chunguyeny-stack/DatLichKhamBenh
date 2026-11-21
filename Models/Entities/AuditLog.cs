using System.ComponentModel.DataAnnotations;

namespace DatLichKhamBenh.Models.Entities
{
    public class AuditLog
    {
        [Key]
        public long Id { get; set; }

        [Required, StringLength(450)]
        public string ActorUserId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Action { get; set; } = string.Empty; // "LockUser", "UnlockUser", ...

        [StringLength(450)]
        public string? TargetUserId { get; set; }

        [StringLength(2000)]
        public string? Details { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}