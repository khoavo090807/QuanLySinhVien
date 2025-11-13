using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace QuanLySinhVien.Models
{
    // ============================================
    // Model: LopThi
    // ============================================
    public class LopThi
    {
        [Display(Name = "Mã lớp thi")]
        public string MaLopThi { get; set; }

        [Display(Name = "Mã đề thi")]
        public string MaDT { get; set; }

        [Display(Name = "Mã giảng viên")]
        public string MaGV { get; set; }

        [Display(Name = "Tên lớp thi")]
        public string TenLopThi { get; set; }

        [Display(Name = "Ngày thi")]
        [DataType(DataType.DateTime)]
        public DateTime NgayThi { get; set; }

        [Display(Name = "Phòng thi")]
        public string PhongThi { get; set; }

        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }

        [Display(Name = "Ngày tạo")]
        [DataType(DataType.DateTime)]
        public DateTime NgayTao { get; set; }

        // Navigation properties
        public string TenDeThi { get; set; }
        public string TenKhoa { get; set; }
        public string TenGiangVien { get; set; }
        public int ThoiGianLamBai { get; set; }
        public int SoDaTham { get; set; }
    }

    // ============================================
    // Model: ChiTietLopThi
    // ============================================
    public class ChiTietLopThi
    {
        public int ID { get; set; }

        [Display(Name = "Mã lớp thi")]
        public string MaLopThi { get; set; }

        [Display(Name = "Mã sinh viên")]
        public string MaSV { get; set; }

        [Display(Name = "Số thứ tự")]
        public int? SoThuTu { get; set; }

        // Navigation properties
        public string HoTenSV { get; set; }
        public string EmailSV { get; set; }
    }

    // ============================================
    // Model: KetQuaThi
    // ============================================
    public class KetQuaThi
    {
        public int ID { get; set; }

        [Display(Name = "Mã lớp thi")]
        public string MaLopThi { get; set; }

        [Display(Name = "Mã sinh viên")]
        public string MaSV { get; set; }

        [Display(Name = "Thời gian bắt đầu")]
        [DataType(DataType.DateTime)]
        public DateTime? ThoiGianBatDau { get; set; }

        [Display(Name = "Thời gian kết thúc")]
        [DataType(DataType.DateTime)]
        public DateTime? ThoiGianKetThuc { get; set; }

        [Display(Name = "Thời gian làm bài")]
        public int? ThoiGianLamBai { get; set; }

        [Display(Name = "Điểm tổng")]
        public float? DiemTong { get; set; }

        [Display(Name = "Xếp loại")]
        public string XepLoai { get; set; }

        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; }

        [Display(Name = "Đã tham gia")]
        public bool DaThamGia { get; set; }

        // Navigation properties
        public string HoTenSV { get; set; }
        public string TenLopThi { get; set; }
    }

    // ============================================
    // Model: CauTraLoiSinhVien
    // ============================================
    public class CauTraLoiSinhVien
    {
        public int ID { get; set; }

        [Display(Name = "Mã lớp thi")]
        public string MaLopThi { get; set; }

        [Display(Name = "Mã sinh viên")]
        public string MaSV { get; set; }

        [Display(Name = "Mã câu")]
        public int MaCau { get; set; }

        [Display(Name = "Đáp án chọn")]
        public char? DapAnChon { get; set; }

        [Display(Name = "Nội dung trả lời")]
        public string NoiDungTraLoi { get; set; }

        [Display(Name = "Điểm câu")]
        public float? DiemCauNay { get; set; }

        [Display(Name = "Thời gian trả lời")]
        [DataType(DataType.DateTime)]
        public DateTime? ThoiGianTraLoi { get; set; }
    }

    // ============================================
    // ViewModel: TaoLopThiViewModel
    // ============================================
    public class TaoLopThiViewModel
    {
        [Display(Name = "Chọn đề thi")]
        [Required(ErrorMessage = "Vui lòng chọn đề thi")]
        public string MaDT { get; set; }

        [Display(Name = "Ngày thi")]
        [Required(ErrorMessage = "Vui lòng chọn ngày thi")]
        [DataType(DataType.DateTime)]
        public DateTime NgayThi { get; set; }

        [Display(Name = "Phòng thi")]
        [Required(ErrorMessage = "Vui lòng nhập phòng thi")]
        public string PhongThi { get; set; }

        [Display(Name = "Giảng viên")]
        public string MaGV { get; set; }

        // Danh sách đề thi
        public List<SelectListItem> DanhSachDeThi { get; set; }

        // Danh sách giáo viên
        public List<SelectListItem> DanhSachGiangVien { get; set; }
    }

    // ============================================
    // ViewModel: BaiThiViewModel
    // ============================================
    public class BaiThiViewModel
    {
        public string MaLopThi { get; set; }
        public string MaDT { get; set; }
        public int ThoiGianLamBai { get; set; }
        public string TenDeThi { get; set; }
        public int TongCau { get; set; }
        public List<CauHoiTracNghiemViewModel> DanhSachCau { get; set; }
        public DateTime ThoiGianBatDau { get; internal set; }
    }

    // ============================================
    // ViewModel: CauHoiTracNghiemViewModel
    // ============================================
    public class CauHoiTracNghiemViewModel
    {
        public int MaCau { get; set; }
        public string NoiDung { get; set; }
        public string DapAnA { get; set; }
        public string DapAnB { get; set; }
        public string DapAnC { get; set; }
        public string DapAnD { get; set; }
        public float Diem { get; set; }
        public int ThuTu { get; set; }
        public char? DapAnChon { get; set; }
    }

    // ============================================
    // ViewModel: KetQuaThiViewModel
    // ============================================
    public class KetQuaThiViewModel
    {
        public string MaLopThi { get; set; }
        public string MaSV { get; set; }
        public string HoTenSV { get; set; }
        public float? DiemTong { get; set; }
        public string XepLoai { get; set; }
        public int? ThoiGianLamBai { get; set; }
        public DateTime? ThoiGianBatDau { get; set; }
        public DateTime? ThoiGianKetThuc { get; set; }
        public string TrangThai { get; set; }
        public List<CauTraLoiChiTiet> CacCauTraLoi { get; set; }
        public string TenLopThi { get; internal set; }
    }

    // ============================================
    // ViewModel: CauTraLoiChiTiet
    // ============================================
    public class CauTraLoiChiTiet
    {
        public int MaCau { get; set; }
        public string NoiDung { get; set; }
        public string DapAnDung { get; set; }
        public char? DapAnChon { get; set; }
        public float Diem { get; set; }
        public bool DungCau { get; set; }
        public float? DiemNhan { get; set; }
    }
}