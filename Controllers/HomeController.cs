
using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class HomeController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        public ActionResult Index()
        {
            // Thống kê tổng số sinh viên
            string querySV = "SELECT COUNT(*) FROM SinhVien";
            ViewBag.TongSinhVien = db.ExecuteScalar(querySV) ?? 0;

            // Thống kê tổng số lớp
            string queryLop = "SELECT COUNT(*) FROM Lop";
            ViewBag.TongLop = db.ExecuteScalar(queryLop) ?? 0;

            // Thống kê tổng số môn học
            string queryMH = "SELECT COUNT(*) FROM MonHoc";
            ViewBag.TongMonHoc = db.ExecuteScalar(queryMH) ?? 0;

            // Thống kê tổng số lượt đăng ký
            string queryDK = "SELECT COUNT(*) FROM DangKyHocPhan";
            ViewBag.TongDangKy = db.ExecuteScalar(queryDK) ?? 0;

            return View();
        }

        // GET: ThongKeHocKy
        public ActionResult ThongKeHocKy(string maHK)
        {
            List<HocKyThongKe> danhSachHK = new List<HocKyThongKe>();
            List<LopHocPhanThongKe> chiTietThongKe = new List<LopHocPhanThongKe>();

            try
            {
                // Lấy danh sách học kỳ có dữ liệu
                string hkQuery = @"
                    SELECT DISTINCT hk.MaHK, hk.TenHK, hk.NgayBatDau, hk.NgayKetThuc,
                           COUNT(lhp.MaLHP) AS SoLopHocPhan
                    FROM HocKy hk
                    LEFT JOIN LopHocPhan lhp ON hk.MaHK = lhp.MaHK
                    GROUP BY hk.MaHK, hk.TenHK, hk.NgayBatDau, hk.NgayKetThuc
                    ORDER BY hk.NgayBatDau DESC";

                DataTable hkDt = db.ExecuteQuery(hkQuery);

                foreach (DataRow hkRow in hkDt.Rows)
                {
                    danhSachHK.Add(new HocKyThongKe
                    {
                        MaHK = hkRow["MaHK"].ToString(),
                        TenHK = hkRow["TenHK"].ToString(),
                        NgayBatDau = Convert.ToDateTime(hkRow["NgayBatDau"]),
                        NgayKetThuc = Convert.ToDateTime(hkRow["NgayKetThuc"]),
                        SoLopHocPhan = Convert.ToInt32(hkRow["SoLopHocPhan"])
                    });
                }

                // Nếu có chọn học kỳ cụ thể, lấy thống kê chi tiết (logic từ sp_BaoCaoHocKy)
                if (!string.IsNullOrEmpty(maHK))
                {
                    // Implement cursor logic từ sp_BaoCaoHocKy (không dùng SiSoToiDa)
                    string detailQuery = @"
                        SELECT
                            lhp.MaLHP,
                            mh.TenMH,
                            hk.TenHK,
                            ISNULL(COUNT(dk.MaSV), 0) AS SiSo
                        FROM LopHocPhan lhp
                        INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                        INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                        LEFT JOIN DangKyHocPhan dk ON lhp.MaLHP = dk.MaLHP
                        WHERE lhp.MaHK = @MaHK
                        GROUP BY lhp.MaLHP, mh.TenMH, hk.TenHK
                        ORDER BY mh.TenMH";

                    SqlParameter[] detailParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaHK", maHK)
                    };

                    DataTable detailDt = db.ExecuteQuery(detailQuery, detailParams);

                    foreach (DataRow detailRow in detailDt.Rows)
                    {
                        chiTietThongKe.Add(new LopHocPhanThongKe
                        {
                            MaLHP = detailRow["MaLHP"].ToString(),
                            TenMH = detailRow["TenMH"].ToString(),
                            TenHK = detailRow["TenHK"].ToString(),
                            SiSoThucTe = Convert.ToInt32(detailRow["SiSo"]),
                            SiSoToiDa = 0, // Không có dữ liệu về sĩ số tối đa
                            TiLeDay = 0  // Không tính tỷ lệ đầy khi không có sĩ số tối đa
                        });
                    }

                    // Tính tổng số sinh viên trong học kỳ
                    string totalStudentsQuery = @"
                        SELECT COUNT(DISTINCT dk.MaSV) AS TotalStudents
                        FROM DangKyHocPhan dk
                        INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                        WHERE lhp.MaHK = @MaHK";

                    SqlParameter[] totalParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaHK", maHK)
                    };

                    int totalStudents = Convert.ToInt32(db.ExecuteScalar(totalStudentsQuery, totalParams));
                    ViewBag.TotalStudents = totalStudents;
                    ViewBag.SelectedHK = maHK;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi tải thống kê: " + ex.Message;
            }

            ViewBag.DanhSachHocKy = danhSachHK;
            ViewBag.ChiTietThongKe = chiTietThongKe;

            return View();
        }
    }
}