using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace QuanLySinhVien.Models
{
    public class Account
    {
        [Display(Name = "Mã tài khoản")]
        public string MaTaiKhoan { get; set; }

        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        public string TenDangNhap { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Loại tài khoản")]
        public string LoaiTaiKhoan { get; set; } // Admin, Lecturer, Student

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; } // true: Active, false: Inactive

        [Display(Name = "Ngày tạo")]
        public DateTime? NgayTao { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }

        // Navigation property
        public string TenHienThi { get; set; }
    }
    public class LoginViewModel
    {
        [Display(Name = "Tên đăng nhập")]
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Display(Name = "Mật khẩu")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Display(Name = "Ghi nhớ tài khoản")]
        public bool GhiNho { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Display(Name = "Mật khẩu hiện tại")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        public string MatKhauHienTai { get; set; }

        [Display(Name = "Mật khẩu mới")]
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải tối thiểu 6 ký tự")]
        public string MatKhauMoi { get; set; }

        [Display(Name = "Xác nhận mật khẩu")]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhauMoi", ErrorMessage = "Mật khẩu không khớp")]
        public string XacNhanMatKhau { get; set; }
    }
}