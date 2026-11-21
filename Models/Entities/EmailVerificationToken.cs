using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    public class EmailVerificationToken
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(6)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiredAt { get; set; }
        public bool IsUsed { get; set; }
    }
}
