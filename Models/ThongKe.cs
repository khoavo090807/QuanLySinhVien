using System;

namespace QuanLySinhVien.Models
{
    public class HocKyThongKe
    {
        public string MaHK { get; set; }
        public string TenHK { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public int SoLopHocPhan { get; set; }
    }

    public class LopHocPhanThongKe
    {
        public string MaLHP { get; set; }
        public string TenMH { get; set; }
        public string TenHK { get; set; }
        public int SiSoThucTe { get; set; }
        public int SiSoToiDa { get; set; }
        public double TiLeDay { get; set; }
    }
}