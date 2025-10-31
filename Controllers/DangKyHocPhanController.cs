using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class DangKyHocPhanController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ============================================
        // HELPER: Parse float từ Database
        // ============================================
        private float? ParseDbFloat(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return null;
            string s = dbValue.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (float.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var vInv))
                return vInv;
            if (float.TryParse(s, NumberStyles.Float, new CultureInfo("vi-VN"), out var vVi))
                return vVi;

            try { return Convert.ToSingle(dbValue); } catch { return null; }
        }

        // ============================================
        // HELPER: Parse float từ Input Form
        // ============================================
        private float? ParseInputFloat(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            if (float.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var valInv))
                return valInv;
            if (float.TryParse(s, NumberStyles.Float, new CultureInfo("vi-VN"), out var valVi))
                return valVi;
            return null;
        }

        // ============================================
        // GET: DangKyHocPhan - Xem danh sách đã đăng ký
        // ============================================
        public ActionResult Index(string MaSV)
        {
            List<DangKyHocPhan> danhSach = new List<DangKyHocPhan>();

            string query = @"SELECT 
                            dk.MaSV, dk.MaLHP, dk.NgayDangKy,
                            dk.DiemChuyenCan, dk.DiemGiuaKy, dk.DiemCuoiKy, dk.DiemTongKet,
                            sv.HoTenSV, mh.TenMH, hk.TenHK
                            FROM DangKyHocPhan dk
                            INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                            INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(MaSV))
            {
                query += " AND dk.MaSV = @MaSV";
                parameters.Add(new SqlParameter("@MaSV", MaSV));
            }
            query += " ORDER BY dk.NgayDangKy DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                DangKyHocPhan dk = new DangKyHocPhan
                {
                    MaSV = row["MaSV"].ToString(),
                    MaLHP = row["MaLHP"].ToString(),
                    NgayDangKy = Convert.ToDateTime(row["NgayDangKy"]),
                    DiemChuyenCan = ParseDbFloat(row["DiemChuyenCan"]),
                    DiemGiuaKy = ParseDbFloat(row["DiemGiuaKy"]),
                    DiemCuoiKy = ParseDbFloat(row["DiemCuoiKy"]),
                    DiemTongKet = ParseDbFloat(row["DiemTongKet"]),
                    HoTenSV = row["HoTenSV"].ToString(),
                    TenMH = row["TenMH"].ToString(),
                    TenHK = row["TenHK"].ToString()
                };
                danhSach.Add(dk);
            }

            ViewBag.MaSV = MaSV;
            ViewBag.TotalCount = danhSach.Count;
            return View(danhSach);
        }

        // ============================================
        // GET: DangKyHocPhan/Create - Hiển thị form
        // ============================================
        public ActionResult Create()
        {
            LoadDanhSachSinhVien();
            LoadDanhSachLopHocPhan();
            return View();
        }

        // ============================================
        // POST: DangKyHocPhan/Create - Xử lý đăng ký
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string MaSV, string MaLHP)
        {
            try
            {
                // Kiểm tra sĩ số lớp chưa đầy (kiểm tra trước khi gọi SP)
                string checkSiSoQuery = @"
                    SELECT 
                        lhp.SoLuongToiDa,
                        (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = @MaLHP) AS SoSinhVienDangKy
                    FROM LopHocPhan lhp
                    WHERE lhp.MaLHP = @MaLHP";
                SqlParameter[] checkSiSoParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };

                DataTable siSoDt = db.ExecuteQuery(checkSiSoQuery, checkSiSoParams);
                if (siSoDt.Rows.Count > 0)
                {
                    DataRow siSoRow = siSoDt.Rows[0];
                    int soLuongToiDa = Convert.ToInt32(siSoRow["SoLuongToiDa"]);
                    int soSinhVienDangKy = Convert.ToInt32(siSoRow["SoSinhVienDangKy"]);

                    if (soSinhVienDangKy >= soLuongToiDa)
                    {
                        TempData["ErrorMessage"] = $"Lỗi: Lớp học phần [{MaLHP}] đã đầy ({soSinhVienDangKy}/{soLuongToiDa})!";
                        LoadDanhSachSinhVien();
                        LoadDanhSachLopHocPhan();
                        return View();
                    }
                }

                // Sử dụng stored procedure sp_DangKyMonHoc
                // SP sẽ tự động kiểm tra: sinh viên tồn tại, lớp tồn tại, trùng đăng ký, tiên quyết
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };

                db.ExecuteStoredProcedureNonQuery("sp_DangKyMonHoc", parameters);

                // Kiểm tra xem đăng ký có thành công không
                string checkQuery = "SELECT COUNT(*) FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";
                SqlParameter[] checkParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };
                int registered = Convert.ToInt32(db.ExecuteScalar(checkQuery, checkParams));

                if (registered > 0)
                {
                    TempData["SuccessMessage"] = "Đăng ký học phần thành công!";
                    return RedirectToAction("Index", new { MaSV = MaSV });
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi: Không thể đăng ký. Vui lòng kiểm tra lại điều kiện.";
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2812)
                {
                    TempData["ErrorMessage"] = "Lỗi: Stored procedure 'sp_DangKyMonHoc' chưa được tạo trong database.";
                }
                else if (ex.Number == 50000 || ex.Message.Contains("LỖI:"))
                {
                    // Lỗi từ stored procedure
                    TempData["ErrorMessage"] = ex.Message.Replace("LỖI:", "").Trim();
                }
                else if (ex.Message.Contains("trg_KiemTraNgayDangKy"))
                {
                    TempData["ErrorMessage"] = "Lỗi: Đã hết hạn đăng ký hoặc học kỳ chưa bắt đầu.";
                }
                else
                {
                    TempData["ErrorMessage"] = $"Lỗi SQL Server: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            LoadDanhSachSinhVien();
            LoadDanhSachLopHocPhan();
            return View();
        }

        // ============================================
        // GET: DangKyHocPhan/Edit - Form nhập điểm
        // ============================================
        public ActionResult Edit(string MaSV, string maLHP)
        {
            if (string.IsNullOrEmpty(MaSV) || string.IsNullOrEmpty(maLHP))
                return HttpNotFound();

            string query = @"SELECT 
                            dk.*, sv.HoTenSV, mh.TenMH
                            FROM DangKyHocPhan dk
                            INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                            INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            SqlParameter[] parameters = {
                new SqlParameter("@MaSV", MaSV),
                new SqlParameter("@MaLHP", maLHP)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0) return HttpNotFound();

            DataRow row = dt.Rows[0];
            DangKyHocPhan dk = new DangKyHocPhan
            {
                MaSV = row["MaSV"].ToString(),
                MaLHP = row["MaLHP"].ToString(),
                NgayDangKy = Convert.ToDateTime(row["NgayDangKy"]),
                DiemChuyenCan = ParseDbFloat(row["DiemChuyenCan"]),
                DiemGiuaKy = ParseDbFloat(row["DiemGiuaKy"]),
                DiemCuoiKy = ParseDbFloat(row["DiemCuoiKy"]),
                DiemTongKet = ParseDbFloat(row["DiemTongKet"]),
                HoTenSV = row["HoTenSV"].ToString(),
                TenMH = row["TenMH"].ToString()
            };
            return View(dk);
        }

        // ============================================
        // POST: DangKyHocPhan/Edit - Lưu điểm
        // ============================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string MaSV, string MaLHP, string DiemChuyenCan, string DiemGiuaKy, string DiemCuoiKy)
        {
            try
            {
                float? diemCC = ParseInputFloat(DiemChuyenCan);
                float? diemGK = ParseInputFloat(DiemGiuaKy);
                float? diemCK = ParseInputFloat(DiemCuoiKy);

                float? diemTongKet = null;
                if (diemCC.HasValue && diemGK.HasValue && diemCK.HasValue)
                    diemTongKet = (float)Math.Round((diemCC.Value * 0.1f) + (diemGK.Value * 0.3f) + (diemCK.Value * 0.6f), 2);

                string query = @"UPDATE DangKyHocPhan 
                               SET DiemChuyenCan = @DiemChuyenCan,
                                   DiemGiuaKy = @DiemGiuaKy,
                                   DiemCuoiKy = @DiemCuoiKy,
                                   DiemTongKet = @DiemTongKet
                               WHERE MaSV = @MaSV AND MaLHP = @MaLHP";

                SqlParameter[] parameters = {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP),
                    new SqlParameter("@DiemChuyenCan", (object)diemCC ?? DBNull.Value),
                    new SqlParameter("@DiemGiuaKy", (object)diemGK ?? DBNull.Value),
                    new SqlParameter("@DiemCuoiKy", (object)diemCK ?? DBNull.Value),
                    new SqlParameter("@DiemTongKet", (object)diemTongKet ?? DBNull.Value)
                };

                int result = db.ExecuteNonQuery(query, parameters);
                if (result > 0)
                    TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
                else
                    TempData["ErrorMessage"] = "Không có thay đổi hoặc không tìm thấy dữ liệu.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Edit", new { MaSV = MaSV, maLHP = MaLHP });
            }
        }

        // ============================================
        // GET: DangKyHocPhan/Delete - Xác nhận xóa
        // ============================================
        public ActionResult Delete(string MaSV, string maLHP)
        {
            if (string.IsNullOrEmpty(MaSV) || string.IsNullOrEmpty(maLHP))
                return HttpNotFound();

            string query = @"SELECT 
                            dk.*, sv.HoTenSV, mh.TenMH, hk.TenHK
                            FROM DangKyHocPhan dk
                            INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                            INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            SqlParameter[] parameters = {
                new SqlParameter("@MaSV", MaSV),
                new SqlParameter("@MaLHP", maLHP)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);
            if (dt.Rows.Count == 0)
                return HttpNotFound();

            DataRow row = dt.Rows[0];
            DangKyHocPhan dk = new DangKyHocPhan
            {
                MaSV = row["MaSV"].ToString(),
                MaLHP = row["MaLHP"].ToString(),
                NgayDangKy = Convert.ToDateTime(row["NgayDangKy"]),
                HoTenSV = row["HoTenSV"].ToString(),
                TenMH = row["TenMH"].ToString(),
                TenHK = row["TenHK"].ToString()
            };
            return View(dk);
        }

        // ============================================
        // POST: DangKyHocPhan/Delete - Hủy đăng ký
        // ============================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string MaSV, string maLHP)
        {
            try
            {
                string query = "DELETE FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";
                SqlParameter[] parameters = {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", maLHP)
                };
                int result = db.ExecuteNonQuery(query, parameters);
                if (result > 0)
                    TempData["SuccessMessage"] = "Hủy đăng ký học phần thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // ============================================
        // HELPER: Load danh sách sinh viên
        // ============================================
        private void LoadDanhSachSinhVien()
        {
            string query = "SELECT MaSV, HoTenSV FROM SinhVien ORDER BY HoTenSV";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaSV"].ToString(),
                    Text = row["MaSV"] + " - " + row["HoTenSV"].ToString()
                });
            }
            ViewBag.DanhSachSinhVien = danhSach;
        }

        // ============================================
        // HELPER: Load danh sách lớp học phần
        // ============================================
        private void LoadDanhSachLopHocPhan()
        {
            string query = @"SELECT lhp.MaLHP, 
                                   mh.TenMH, 
                                   hk.TenHK, 
                                   gv.HoTenGV,
                                   lhp.SoLuongToiDa,
                                   hk.NgayBatDau,
                                   hk.NgayKetThuc,
                                   (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = lhp.MaLHP) AS SoSinhVienDangKy,
                                   CASE 
                                       WHEN GETDATE() < hk.NgayBatDau THEN '(Chưa mở) ' 
                                       WHEN GETDATE() > hk.NgayKetThuc THEN '(Đã đóng) '
                                       ELSE '(Đang mở) '
                                   END AS TrangThaiHK
                            FROM LopHocPhan lhp
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                            ORDER BY hk.NgayBatDau DESC, mh.TenMH";

            DataTable dt = db.ExecuteQuery(query);
            List<SelectListItem> danhSach = new List<SelectListItem>();

            foreach (DataRow row in dt.Rows)
            {
                string trangThaiHK = row["TrangThaiHK"].ToString();
                int soSinhVienDangKy = Convert.ToInt32(row["SoSinhVienDangKy"]);
                int soLuongToiDa = Convert.ToInt32(row["SoLuongToiDa"]);

                // Kiểm tra nếu lớp đã đầy
                string dayDuText = soSinhVienDangKy >= soLuongToiDa ? " [ĐÃ ĐẦY]" : "";

                // Hiển thị số lượng sinh viên đã đăng ký
                string soSVText = $" ({soSinhVienDangKy}/{soLuongToiDa})";

                danhSach.Add(new SelectListItem
                {
                    Value = row["MaLHP"].ToString(),
                    Text = row["MaLHP"] + " - " +
                           row["TenMH"] + " (" + row["TenHK"] + ") " +
                           trangThaiHK + soSVText + dayDuText,
                    Disabled = soSinhVienDangKy >= soLuongToiDa  // Vô hiệu hóa nếu đã đầy
                });
            }

            ViewBag.DanhSachLopHocPhan = danhSach;
        }
    }
}