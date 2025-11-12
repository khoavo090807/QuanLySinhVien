using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    public class CauHoi
    {
        [Display(Name = "Mã câu hỏi")]
        public int MaCauHoi { get; set; }

        [Display(Name = "Mã đề thi")]
        public string MaDT { get; set; }

        [Display(Name = "Nội dung câu hỏi")]
        [Required(ErrorMessage = "Nội dung câu hỏi không được để trống")]
        public string NoiDungCau { get; set; }

        [Display(Name = "Loại câu")]
        [Required(ErrorMessage = "Vui lòng chọn loại câu")]
        public string LoaiCau { get; set; }  // TN: Trắc nghiệm, TL: Tự luận

        [Display(Name = "Đáp án đúng")]
        public string DapAnDung { get; set; }

        [Display(Name = "Điểm câu")]
        [Range(0.1, 100)]
        public float DiemCau { get; set; }

        [Display(Name = "Thứ tự")]
        public int ThuTu { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime NgayTao { get; set; }

        // Navigation properties
        public List<DapAnTracNghiem> DanhSachDapAn { get; set; }
    }
}
