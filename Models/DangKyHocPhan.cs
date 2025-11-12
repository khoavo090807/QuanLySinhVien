using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace QuanLySinhVien.Models
{
    public class DangKyHocPhan
    {
        [Display(Name = "Mã sinh viên")]
        public string MaSV { get; set; }

        [Display(Name = "Mã lớp học phần")]
        public string MaLHP { get; set; }

        [Display(Name = "Ngày đăng ký")]
        [DataType(DataType.DateTime)]
        public DateTime NgayDangKy { get; set; }

        [Display(Name = "Điểm chuyên cần")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public float? DiemChuyenCan { get; set; }

        [Display(Name = "Điểm giữa kỳ")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public float? DiemGiuaKy { get; set; }

        [Display(Name = "Điểm cuối kỳ")]
        [Range(0, 10, ErrorMessage = "Điểm phải từ 0 đến 10")]
        public float? DiemCuoiKy { get; set; }

        [Display(Name = "Điểm tổng kết")]
        public float? DiemTongKet { get; set; }

        // Navigation properties
        public string HoTenSV { get; set; }
        public string TenMH { get; set; }
        public string TenHK { get; set; }
        public string TenKhoa { get; set; }
    }

    
}