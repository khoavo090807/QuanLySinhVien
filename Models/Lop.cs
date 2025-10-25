using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLySinhVien.Models
{
    public class Lop
    {
        [Display(Name = "Mã lớp")]
        [Required(ErrorMessage = "Mã lớp không được để trống")]
        public string MaLop { get; set; }

        [Display(Name = "Tên lớp")]
        [Required(ErrorMessage = "Tên lớp không được để trống")]
        [StringLength(100)]
        public string TenLop { get; set; }

        [Display(Name = "Sĩ số")]
        public int SiSo { get; set; }

        [Display(Name = "Mã khoa")]
        public string MaKhoa { get; set; }

        [Display(Name = "Mã hệ đào tạo")]
        public string MaHeDT { get; set; }

        // Navigation properties
        public string TenKhoa { get; set; }
        public string TenHeDT { get; set; }
    }
}