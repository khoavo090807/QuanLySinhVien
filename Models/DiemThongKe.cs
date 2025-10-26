using System;

namespace QuanLySinhVien.Models
{
    public class DiemThongKe
    {
        public string MaSV { get; set; }
        public string HoTenSV { get; set; }
        public string MaLHP { get; set; }
        public string TenMH { get; set; }
        public int SoTinChi { get; set; }

        public float? DiemChuyenCan { get; set; }
        public float? DiemGiuaKy { get; set; }
        public float? DiemCuoiKy { get; set; }
        public float? DiemTongKet { get; set; }

        public string XepLoai { get; set; }
        public DateTime NgayDangKy { get; set; }
    }
}