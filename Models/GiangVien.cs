using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLySinhVien.Models
{
    public class GiangVien
    {
        [Display(Name = "Mã giáo viên")]
        [Required(ErrorMessage = "Mã giáo viên không được để trống")]
        public string MaGV { get; set; }

        [Display(Name = "Họ tên giáo viên")]
        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTenGV { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public string GioiTinh { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDT { get; set; }

        [Display(Name = "Mã khoa")]
        public string MaKhoa { get; set; }

        [Display(Name = "Mã chức vụ")]
        public string MaChucVu { get; set; }

        // Navigation properties
        public string TenKhoa { get; set; }
        public string TenChucVu { get; set; }
    }
}