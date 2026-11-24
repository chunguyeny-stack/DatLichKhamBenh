using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DatLichKhamBenh.Models.Entities;
using DatLichKhamBenh.Models.Enums;

namespace DatLichKhamBenh.Models.ViewModels
{
    public class DatLichViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn bác sĩ khám.")]
        [Display(Name = "Bác sĩ")]
        public int BacSiId { get; set; }

        [Display(Name = "Chuyên khoa")] public int? ChuyenKhoaId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày khám.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày khám")]
        public DateTime NgayKham { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Vui lòng chọn ca khám.")]
        [Display(Name = "Ca khám")]
        public CaKham CaKham { get; set; } = CaKham.Sang;

        [Required(ErrorMessage = "Vui lòng chọn khung giờ khám.")]
        [Display(Name = "Khung giờ")]
        public string KhungGio { get; set; } = "08:00 - 09:00";

        [Required(ErrorMessage = "Vui lòng nhập triệu chứng hoặc lý do khám.")]
        [StringLength(1000, ErrorMessage = "Triệu chứng tối đa 1000 ký tự.")]
        [Display(Name = "Triệu chứng")]
        public string GhiChu { get; set; } = string.Empty;

        public decimal PhiCoBan { get; set; } = 200000m;
        public decimal TongPhi { get; set; } = 200000m;

        public List<Specialty> ChuyenKhoas { get; set; } = [];
        public List<Doctor> BacSis { get; set; } = [];
    }
}