
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DatLichKhamBenh.Models.Entities
{
    [Table("Appointment")]
    public class Appointment
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }

        [Column("PatientId")]
        public int PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual Patient? Patient { get; set; }

        [Column("DoctorId")]
        public int DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual Doctor? Doctor { get; set; }

        [Column("AppointmentDate", TypeName = "date")]
        public DateTime AppointmentDate { get; set; }

        [Column("TimeSlot")]
        [MaxLength(50)]
        public string TimeSlot { get; set; } = string.Empty;

        [Column("Symptoms")]
        public string? Symptoms { get; set; }

        [Column("Status")]
        [MaxLength(50)]
        public string Status { get; set; } = "Chờ";

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("ConsultationFee", TypeName = "decimal(18, 2)")]
        public decimal ConsultationFee { get; set; } = 0m;

        [Column("MedicineFee", TypeName = "decimal(18, 2)")]
        public decimal MedicineFee { get; set; } = 0m;

        [Column("ServiceFee", TypeName = "decimal(18, 2)")]
        public decimal ServiceFee { get; set; } = 0m;

        [NotMapped]
        public int MaLichHen => Id;

        [NotMapped]
        public decimal TotalFee => ConsultationFee + MedicineFee + ServiceFee;
    }
}