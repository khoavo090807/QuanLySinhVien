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
        public ActionResult Index(string maSV)
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
            if (!string.IsNullOrEmpty(maSV))
            {
                query += " AND dk.MaSV = @MaSV";
                parameters.Add(new SqlParameter("@MaSV", maSV));
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

            ViewBag.MaSV = maSV;
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
                // ✅ 1. Kiểm tra sinh viên có tồn tại không
                string checkSVQuery = "SELECT COUNT(*) FROM SinhVien WHERE MaSV = @MaSV";
                SqlParameter[] checkSVParams = new SqlParameter[] { new SqlParameter("@MaSV", MaSV) };
                int svCount = Convert.ToInt32(db.ExecuteScalar(checkSVQuery, checkSVParams));

                if (svCount == 0)
                {
                    TempData["ErrorMessage"] = $"Lỗi: Sinh viên [{MaSV}] không tồn tại!";
                    LoadDanhSachSinhVien();
                    LoadDanhSachLopHocPhan();
                    return View();
                }

                // ✅ 2. Kiểm tra lớp học phần tồn tại
                string checkLHPQuery = "SELECT COUNT(*) FROM LopHocPhan WHERE MaLHP = @MaLHP";
                SqlParameter[] checkLHPParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };
                int lhpCount = Convert.ToInt32(db.ExecuteScalar(checkLHPQuery, checkLHPParams));

                if (lhpCount == 0)
                {
                    TempData["ErrorMessage"] = $"Lỗi: Mã lớp học phần [{MaLHP}] không tồn tại!";
                    LoadDanhSachSinhVien();
                    LoadDanhSachLopHocPhan();
                    return View();
                }

                // ✅ 3. Kiểm tra sinh viên đã đăng ký lớp này chưa
                string checkDupQuery = "SELECT COUNT(*) FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";
                SqlParameter[] checkDupParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };
                int dupCount = Convert.ToInt32(db.ExecuteScalar(checkDupQuery, checkDupParams));

                if (dupCount > 0)
                {
                    TempData["ErrorMessage"] = $"Lỗi: Sinh viên [{MaSV}] đã đăng ký lớp [{MaLHP}] này rồi!";
                    LoadDanhSachSinhVien();
                    LoadDanhSachLopHocPhan();
                    return View();
                }

                // ✅ 4. Kiểm tra thời gian đăng ký (Học kỳ có đang mở không)
                string dateCheckQuery = @"
                    SELECT
                        CASE
                            WHEN GETDATE() < hk.NgayBatDau THEN 0
                            WHEN GETDATE() > hk.NgayKetThuc THEN 1
                            ELSE 2
                        END AS TrangThai,
                        hk.NgayBatDau,
                        hk.NgayKetThuc,
                        hk.TenHK
                    FROM LopHocPhan lhp
                    INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                    WHERE lhp.MaLHP = @MaLHP";
                SqlParameter[] dateCheckParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };

                DataTable dateDt = db.ExecuteQuery(dateCheckQuery, dateCheckParams);
                if (dateDt.Rows.Count > 0)
                {
                    DataRow dateRow = dateDt.Rows[0];
                    int trangThai = Convert.ToInt32(dateRow["TrangThai"]);
                    string tenHK = dateRow["TenHK"].ToString();
                    DateTime ngayBatDau = Convert.ToDateTime(dateRow["NgayBatDau"]);
                    DateTime ngayKetThuc = Convert.ToDateTime(dateRow["NgayKetThuc"]);

                    if (trangThai == 0)
                    {
                        TempData["ErrorMessage"] = $"Lỗi: Học kỳ [{tenHK}] chưa bắt đầu. Thời gian đăng ký: từ {ngayBatDau:dd/MM/yyyy} đến {ngayKetThuc:dd/MM/yyyy}.";
                        LoadDanhSachSinhVien();
                        LoadDanhSachLopHocPhan();
                        return View();
                    }
                    else if (trangThai == 1)
                    {
                        TempData["ErrorMessage"] = $"Lỗi: Học kỳ [{tenHK}] đã kết thúc. Thời gian đăng ký: từ {ngayBatDau:dd/MM/yyyy} đến {ngayKetThuc:dd/MM/yyyy}.";
                        LoadDanhSachSinhVien();
                        LoadDanhSachLopHocPhan();
                        return View();
                    }
                }

                // ✅ 5. Kiểm tra sĩ số lớp chưa đầy
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

                // ✅ 6. Kiểm tra tiên quyết (nếu có)
                string getMHQuery = "SELECT MaMH FROM LopHocPhan WHERE MaLHP = @MaLHP";
                SqlParameter[] getMHParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };
                object maMHObj = db.ExecuteScalar(getMHQuery, getMHParams);

                if (maMHObj != null)
                {
                    string maMH = maMHObj.ToString();

                    // Gọi hàm kiểm tra tiên quyết
                    string prereqQuery = "SELECT dbo.fn_KiemTraTienQuyet(@MaSV, @MaMH)";
                    SqlParameter[] prereqParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaSV", MaSV),
                        new SqlParameter("@MaMH", maMH)
                    };

                    try
                    {
                        int prereqResult = Convert.ToInt32(db.ExecuteScalar(prereqQuery, prereqParams));

                        if (prereqResult == 0)
                        {
                            TempData["ErrorMessage"] = $"Lỗi: Sinh viên [{MaSV}] không đủ điều kiện tiên quyết để đăng ký môn [{maMH}]!";
                            LoadDanhSachSinhVien();
                            LoadDanhSachLopHocPhan();
                            return View();
                        }
                    }
                    catch (Exception exPrereq)
                    {
                        // Nếu hàm không tồn tại, bỏ qua kiểm tra tiên quyết
                        TempData["ErrorMessage"] = $"Cảnh báo: Không thể kiểm tra tiên quyết. {exPrereq.Message}";
                    }
                }

                // ✅ 7. ĐĂNG KÝ THÀNH CÔNG - Thêm vào database
                string insertQuery = @"INSERT INTO DangKyHocPhan (MaSV, MaLHP, NgayDangKy)
                                       VALUES (@MaSV, @MaLHP, GETDATE())";
                SqlParameter[] insertParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };

                int result = db.ExecuteNonQuery(insertQuery, insertParams);

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Đăng ký học phần thành công!";
                    return RedirectToAction("Index", new { maSV = MaSV });
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi: Không thể đăng ký. Vui lòng thử lại!";
                }
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = $"Lỗi SQL Server: {ex.Message}";
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
        public ActionResult Edit(string maSV, string maLHP)
        {
            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(maLHP))
                return HttpNotFound();

            string query = @"SELECT 
                            dk.*, sv.HoTenSV, mh.TenMH
                            FROM DangKyHocPhan dk
                            INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                            INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            SqlParameter[] parameters = {
                new SqlParameter("@MaSV", maSV),
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
                return RedirectToAction("Edit", new { maSV = MaSV, maLHP = MaLHP });
            }
        }

        // ============================================
        // GET: DangKyHocPhan/Delete - Xác nhận xóa
        // ============================================
        public ActionResult Delete(string maSV, string maLHP)
        {
            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(maLHP))
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
                new SqlParameter("@MaSV", maSV),
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
        public ActionResult DeleteConfirmed(string maSV, string maLHP)
        {
            try
            {
                string query = "DELETE FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";
                SqlParameter[] parameters = {
                    new SqlParameter("@MaSV", maSV),
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