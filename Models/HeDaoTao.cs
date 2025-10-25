using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLySinhVien.Models
{
    public class HeDaoTao
    {
        [Display(Name = "Mã hệ đào tạo")]
        public string MaHeDT { get; set; }

        [Display(Name = "Tên hệ đào tạo")]
        public string TenHeDT { get; set; }
    }
}