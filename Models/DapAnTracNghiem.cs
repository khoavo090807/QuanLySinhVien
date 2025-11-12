using System;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class DapAnTracNghiem
    {
        [Display(Name = "Mã đáp án")]
        public int MaDapAn { get; set; }

        [Display(Name = "Mã câu hỏi")]
        public int MaCauHoi { get; set; }

        [Display(Name = "Thứ tự")]
        public int ThuTu { get; set; }

        [Display(Name = "Nội dung đáp án")]
        [Required(ErrorMessage = "Nội dung đáp án không được để trống")]
        public string NoiDungDapAn { get; set; }

        [Display(Name = "Là đáp án đúng")]
        public bool LaDapAnDung { get; set; }
    }
}