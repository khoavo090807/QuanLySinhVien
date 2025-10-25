using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLySinhVien.Models
{
    public class MonHoc
    {
        [Display(Name = "Mã môn học")]
        [Required(ErrorMessage = "Mã môn học không được để trống")]
        public string MaMH { get; set; }

        [Display(Name = "Tên môn học")]
        [Required(ErrorMessage = "Tên môn học không được để trống")]
        [StringLength(100)]
        public string TenMH { get; set; }

        [Display(Name = "Số tín chỉ")]
        [Required(ErrorMessage = "Số tín chỉ không được để trống")]
        [Range(1, 10, ErrorMessage = "Số tín chỉ phải từ 1 đến 10")]
        public int SoTinChi { get; set; }
    }
}