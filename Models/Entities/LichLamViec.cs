using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("DoctorSchedule")]
    public class DoctorSchedule
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("DoctorId")]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor Doctor { get; set; } = null!;

        [Column("WorkDate", TypeName = "date")]
        public DateTime WorkDate { get; set; }

        [Column("Shift")]
        [MaxLength(50)]
        public string Shift { get; set; } = string.Empty;

        [Column("RoomName")]
        [MaxLength(100)]
        public string? RoomName { get; set; }

        [Column("TimeSlot")]
        [MaxLength(50)]
        public string TimeSlot { get; set; } = string.Empty;

        [Column("IsAvailable")]
        public bool IsAvailable { get; set; } = true;

        [Column("AbsenceReason")]
        [MaxLength(500)]
        public string? AbsenceReason { get; set; }

        [NotMapped]
        public int MaLichLamViec => Id;
    }
}