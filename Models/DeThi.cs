using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class DeThi
    {
        [Display(Name = "Mã đề thi")]
        [Required(ErrorMessage = "Mã đề thi không được để trống")]
        public string MaDT { get; set; }

        [Display(Name = "Tên đề thi")]
        [Required(ErrorMessage = "Tên đề thi không được để trống")]
        [StringLength(200)]
        public string TenDT { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(500)]
        public string MoTa { get; set; }

        [Display(Name = "Khoa")]
        [Required(ErrorMessage = "Vui lòng chọn khoa")]
        public string MaKhoa { get; set; }

        [Display(Name = "Số câu hỏi")]
        public int SoCau { get; set; }

        [Display(Name = "Thời gian làm bài (phút)")]
        [Range(1, 480, ErrorMessage = "Thời gian từ 1-480 phút")]
        public int? ThoiGianLamBai { get; set; }

        [Display(Name = "Trạng thái")]
        public bool TrangThai { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        [Display(Name = "Ngày cập nhật")]
        public DateTime? NgayCapNhat { get; set; }

        // Navigation properties
        public string TenKhoa { get; set; }
        public List<CauHoi> DanhSachCauHoi { get; set; }
    }
}