using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLySinhVien.Models
{
    public class Khoa
    {
        [Display(Name = "Mã khoa")]
        public string MaKhoa { get; set; }

        [Display(Name = "Tên khoa")]
        public string TenKhoa { get; set; }

        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }
    }
}