using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("Service")]
    public class Service
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("SpecialtyId")]
        public int SpecialtyId { get; set; }

        [ForeignKey(nameof(SpecialtyId))]
        public virtual Specialty? Specialty { get; set; }

        [Required]
        [Column("Name")]
        [MaxLength(250)]
        public string Name { get; set; } = string.Empty;

        [Column("Price", TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } = 0m;

        [NotMapped]
        public int MaDichVu => Id;
    }
}