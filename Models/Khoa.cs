using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class Khoa
    {
        [Display(Name = "Mã khoa")]
        [Required(ErrorMessage = "Mã khoa không được để trống")]
        public string MaKhoa { get; set; }

        [Display(Name = "Tên khoa")]
        [Required(ErrorMessage = "Tên khoa không được để trống")]
        [StringLength(100)]
        public string TenKhoa { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string SoDienThoai { get; set; }

        

        // Navigation properties
        public virtual ICollection<Lop> DanhSachLop { get; set; }
        public virtual ICollection<GiangVien> DanhSachGiangVien { get; set; }
    }
}