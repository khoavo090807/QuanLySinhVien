// ============================================
// FILE: Models/LopHocPhan.cs
// CHỨC NĂNG: Model Lớp Học Phần
// ============================================

using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class LopHocPhan
    {
        [Display(Name = "Mã lớp học phần")]
        [Required(ErrorMessage = "Mã lớp học phần không được để trống")]
        public string MaLHP { get; set; }

        [Display(Name = "Mã môn học")]
        [Required(ErrorMessage = "Môn học không được để trống")]
        public string MaMH { get; set; }

        [Display(Name = "Mã học kỳ")]
        [Required(ErrorMessage = "Học kỳ không được để trống")]
        public string MaHK { get; set; }

        [Display(Name = "Mã giáo viên")]
        public string MaGV { get; set; }

        [Display(Name = "Phòng học")]
        public string PhongHoc { get; set; }

        [Display(Name = "Sĩ số tối đa")]
        [Required(ErrorMessage = "Sĩ số tối đa không được để trống")]
        [Range(1, 100, ErrorMessage = "Sĩ số tối đa phải từ 1 đến 100")]
        public int SoLuongToiDa { get; set; }

        // Navigation properties & Display properties
        public string TenMH { get; set; }
        public string TenHK { get; set; }
        public string TenGiangVien { get; set; }
        public int SoSinhVienDangKy { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public string TrangThai { get; set; }  // Chưa mở, Đang mở, Đã đóng

        // Tính toán các thuộc tính
        public int SoChoConLai
        {
            get { return SoLuongToiDa - SoSinhVienDangKy; }
        }

        public double TiLeDay
        {
            get
            {
                if (SoLuongToiDa == 0) return 0;
                return Math.Round((double)SoSinhVienDangKy / SoLuongToiDa * 100, 2);
            }
        }

        public bool DayDu
        {
            get { return SoSinhVienDangKy >= SoLuongToiDa; }
        }

        public bool CoTheNhapDangKy
        {
            get { return TrangThai == "Đang mở" && !DayDu; }
        }
    }
}