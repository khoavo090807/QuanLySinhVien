// ============================================
// FILE: Models/HocKy.cs
// CHỨC NĂNG: Model Học kỳ
// ============================================

using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class HocKy
    {
        [Display(Name = "Mã học kỳ")]
        [Required(ErrorMessage = "Mã học kỳ không được để trống")]
        public string MaHK { get; set; }

        [Display(Name = "Tên học kỳ")]
        [Required(ErrorMessage = "Tên học kỳ không được để trống")]
        [StringLength(50)]
        public string TenHK { get; set; }

        [Display(Name = "Năm học")]
        [Required(ErrorMessage = "Năm học không được để trống")]
        [RegularExpression(@"\d{4}-\d{4}", ErrorMessage = "Năm học phải có format YYYY-YYYY (VD: 2024-2025)")]
        public string NamHoc { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Display(Name = "Ngày kết thúc")]
        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; }

        // Thêm các properties để hiển thị thông tin liên quan
        public int SoLopHocPhan { get; set; }
        public int SoSinhVienDangKy { get; set; }

        // Kiểm tra trạng thái của học kỳ
        public string GetTrangThai()
        {
            DateTime now = DateTime.Now;
            if (now < NgayBatDau)
                return "Chưa bắt đầu";
            else if (now > NgayKetThuc)
                return "Đã kết thúc";
            else
                return "Đang mở";
        }

        // Kiểm tra có thể đăng ký không
        public bool CoTheNhapDangKy()
        {
            DateTime now = DateTime.Now;
            return now >= NgayBatDau && now <= NgayKetThuc;
        }

        // Tính số ngày còn lại
        public int SoNgayConLai()
        {
            TimeSpan ts = NgayKetThuc - DateTime.Now;
            return ts.Days;
        }
    }
}