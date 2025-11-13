using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class CauHoiTracNghiem
    {
        [Key]
        public int MaCau { get; set; }

        [Required(ErrorMessage = "Mã đề thi không được để trống")]
        public string MaDT { get; set; }

        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        [StringLength(1000, ErrorMessage = "Nội dung tối đa 1000 ký tự")]
        public string NoiDung { get; set; }

        [Required(ErrorMessage = "Đáp án A không được để trống")]
        [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự")]
        public string DapAnA { get; set; }

        [Required(ErrorMessage = "Đáp án B không được để trống")]
        [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự")]
        public string DapAnB { get; set; }

        [Required(ErrorMessage = "Đáp án C không được để trống")]
        [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự")]
        public string DapAnC { get; set; }

        [Required(ErrorMessage = "Đáp án D không được để trống")]
        [StringLength(500, ErrorMessage = "Đáp án tối đa 500 ký tự")]
        public string DapAnD { get; set; }

        [Required(ErrorMessage = "Đáp án đúng không được để trống")]
        [RegularExpression(@"[A-D]", ErrorMessage = "Đáp án đúng phải là A, B, C hoặc D")]
        public string DapAnDung { get; set; }

        [Range(0.5, 10, ErrorMessage = "Điểm phải từ 0.5 đến 10")]
        public float Diem { get; set; }

        public int ThuTu { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        // Navigation Property
        public virtual DeThi DeThi { get; set; }
    }
}