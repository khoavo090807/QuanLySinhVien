using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    [Authorize]
    public class DangKyHocPhanController : Controller
    {
        private readonly DatabaseHelper db = new DatabaseHelper();
        private string CurrentUserId => User.Identity.Name;
        private string CurrentRole => GetCurrentUserRole();

        // ============================================
        // LẤY VAI TRÒ NGƯỜI DÙNG
        // ============================================
        private string GetCurrentUserRole()
        {
            var dt = db.ExecuteQuery(
                "SELECT LoaiTaiKhoan FROM Account WHERE TenDangNhap = @TenDangNhap",
                new SqlParameter[] { new SqlParameter("@TenDangNhap", CurrentUserId) }
            );

            return dt.Rows.Count > 0 ? dt.Rows[0]["LoaiTaiKhoan"].ToString() : "Unknown";
        }

        private string GetMaSVFromAccount(string tenDangNhap)
        {
            var dt = db.ExecuteQuery(
                "SELECT MaTaiKhoan FROM Account WHERE TenDangNhap = @TenDangNhap",
                new SqlParameter[] { new SqlParameter("@TenDangNhap", tenDangNhap) }
            );

            return dt.Rows.Count > 0 ? dt.Rows[0]["MaTaiKhoan"].ToString() : null;
        }

        // ============================================
        // PARSE FLOAT
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
        // INDEX - Danh sách đăng ký
        // ============================================
        public ActionResult Index(string MaSV = null)
        {
            var danhSach = new List<DangKyHocPhan>();
            string query = @"
                SELECT dk.MaSV, dk.MaLHP, dk.NgayDangKy,
                       dk.DiemChuyenCan, dk.DiemGiuaKy, dk.DiemCuoiKy, dk.DiemTongKet,
                       sv.HoTenSV, mh.TenMH, hk.TenHK
                FROM DangKyHocPhan dk
                INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (CurrentRole == "Student")
            {
                string maSV = GetMaSVFromAccount(CurrentUserId);
                if (string.IsNullOrEmpty(maSV))
                    return HttpNotFound("Không tìm thấy sinh viên.");

                query += " AND dk.MaSV = @MaSV";
                parameters.Add(new SqlParameter("@MaSV", maSV));
                ViewBag.IsStudent = true;
                ViewBag.CurrentMaSV = maSV;
            }
            else if (!string.IsNullOrEmpty(MaSV))
            {
                query += " AND dk.MaSV = @MaSV";
                parameters.Add(new SqlParameter("@MaSV", MaSV));
                ViewBag.IsStudent = false;
            }
            else
            {
                ViewBag.IsStudent = false;
            }

            query += " ORDER BY dk.NgayDangKy DESC";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());
            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new DangKyHocPhan
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
                });
            }

            ViewBag.MaSV = MaSV ?? (CurrentRole == "Student" ? GetMaSVFromAccount(CurrentUserId) : null);
            ViewBag.TotalCount = danhSach.Count;
            ViewBag.CurrentRole = CurrentRole;
            return View(danhSach);
        }

        // ============================================
        // CREATE - Form
        // ============================================
        public ActionResult Create()
        {
            if (CurrentRole == "Student")
            {
                string maSV = GetMaSVFromAccount(CurrentUserId);
                if (string.IsNullOrEmpty(maSV)) return HttpNotFound();

                ViewBag.MaSV = maSV;
                LoadDanhSachLopHocPhan_TheoKhoaSV(maSV);
            }
            else
            {
                LoadDanhSachSinhVien();
                LoadDanhSachLopHocPhan();
            }

            return View();
        }

        // ============================================
        // CREATE - POST
        // ============================================
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string MaSV, string MaLHP)
        {
            if (CurrentRole == "Student")
            {
                MaSV = GetMaSVFromAccount(CurrentUserId);
                if (string.IsNullOrEmpty(MaSV)) return HttpNotFound();
            }

            try
            {
                // 1. KIỂM TRA THỜI GIAN HỌC KỲ
                string checkThoiGianQuery = @"
            SELECT hk.NgayBatDau, hk.NgayKetThuc
            FROM LopHocPhan lhp
            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
            WHERE lhp.MaLHP = @MaLHP";

                var thoiGianDt = db.ExecuteQuery(checkThoiGianQuery,
                    new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) });

                if (thoiGianDt.Rows.Count == 0)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lớp học phần.";
                    return RedirectToCreateWithData(MaSV);
                }

                DateTime ngayBatDau = Convert.ToDateTime(thoiGianDt.Rows[0]["NgayBatDau"]);
                DateTime ngayKetThuc = Convert.ToDateTime(thoiGianDt.Rows[0]["NgayKetThuc"]);
                DateTime ngayHienTai = DateTime.Now;

                if (ngayHienTai < ngayBatDau)
                {
                    TempData["ErrorMessage"] = $"Lớp [{MaLHP}] chưa mở đăng ký (từ {ngayBatDau:dd/MM/yyyy}).";
                    return RedirectToCreateWithData(MaSV);
                }

                if (ngayHienTai > ngayKetThuc)
                {
                    TempData["ErrorMessage"] = $"Lớp [{MaLHP}] đã đóng đăng ký (đến {ngayKetThuc:dd/MM/yyyy}).";
                    return RedirectToCreateWithData(MaSV);
                }

                // 2. KIỂM TRA SĨ SỐ
                string checkSiSoQuery = @"
            SELECT lhp.SoLuongToiDa,
                   (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = @MaLHP) AS SoSinhVienDangKy
            FROM LopHocPhan lhp WHERE lhp.MaLHP = @MaLHP";

                var siSoDt = db.ExecuteQuery(checkSiSoQuery,
                    new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) });

                if (siSoDt.Rows.Count > 0)
                {
                    int soLuongToiDa = Convert.ToInt32(siSoDt.Rows[0]["SoLuongToiDa"]);
                    int soDangKy = Convert.ToInt32(siSoDt.Rows[0]["SoSinhVienDangKy"]);

                    if (soDangKy >= soLuongToiDa)
                    {
                        TempData["ErrorMessage"] = $"Lớp [{MaLHP}] đã đầy ({soDangKy}/{soLuongToiDa})!";
                        return RedirectToCreateWithData(MaSV);
                    }
                }

                // 3. GỌI SP ĐĂNG KÝ
                db.ExecuteStoredProcedureNonQuery("sp_DangKyMonHoc",
                    new SqlParameter[] {
                new SqlParameter("@MaSV", MaSV),
                new SqlParameter("@MaLHP", MaLHP)
                    });

                // 4. KIỂM TRA THÀNH CÔNG
                int registered = Convert.ToInt32(db.ExecuteScalar(
                    "SELECT COUNT(*) FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP",
                    new SqlParameter[] {
                new SqlParameter("@MaSV", MaSV),
                new SqlParameter("@MaLHP", MaLHP)
                    }));

                if (registered > 0)
                {
                    TempData["SuccessMessage"] = "Đăng ký thành công!";
                    return RedirectToAction("Index", new { MaSV = CurrentRole == "Student" ? (string)null : MaSV });
                }
                else
                {
                    TempData["ErrorMessage"] = "Đăng ký thất bại. Kiểm tra điều kiện.";
                }
            }
            catch (SqlException ex)
            {
                TempData["ErrorMessage"] = ex.Message.Contains("LỖI:")
                    ? ex.Message.Replace("LỖI:", "").Trim()
                    : ex.Message;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToCreateWithData(MaSV);
        }

        private ActionResult RedirectToCreateWithData(string maSV)
        {
            if (CurrentRole == "Student")
            {
                LoadDanhSachLopHocPhan_TheoKhoaSV(maSV);
            }
            else
            {
                LoadDanhSachSinhVien();
                LoadDanhSachLopHocPhan();
                ViewBag.MaSV = maSV;
            }
            
            return View("Create");
        }

        // ============================================
        // EDIT
        // ============================================
        public ActionResult Edit(string MaSV, string maLHP)
        {
            if (CurrentRole == "Student" && MaSV != GetMaSVFromAccount(CurrentUserId))
                return HttpNotFound();

            string query = @"
                SELECT dk.*, sv.HoTenSV, mh.TenMH
                FROM DangKyHocPhan dk
                INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            var dt = db.ExecuteQuery(query,
                new SqlParameter[] {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", maLHP)
                });

            if (dt.Rows.Count == 0) return HttpNotFound();

            DataRow row = dt.Rows[0];
            return View(new DangKyHocPhan
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
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(string MaSV, string MaLHP, string DiemChuyenCan, string DiemGiuaKy, string DiemCuoiKy)
        {
            if (CurrentRole == "Student" && MaSV != GetMaSVFromAccount(CurrentUserId))
                return HttpNotFound();

            try
            {
                float? diemCC = ParseInputFloat(DiemChuyenCan);
                float? diemGK = ParseInputFloat(DiemGiuaKy);
                float? diemCK = ParseInputFloat(DiemCuoiKy);

                float? diemTongKet = null;
                if (diemCC.HasValue && diemGK.HasValue && diemCK.HasValue)
                    diemTongKet = (float)Math.Round(diemCC.Value * 0.1f + diemGK.Value * 0.3f + diemCK.Value * 0.6f, 2);

                string query = @"
                    UPDATE DangKyHocPhan 
                    SET DiemChuyenCan = @DiemChuyenCan,
                        DiemGiuaKy = @DiemGiuaKy,
                        DiemCuoiKy = @DiemCuoiKy,
                        DiemTongKet = @DiemTongKet
                    WHERE MaSV = @MaSV AND MaLHP = @MaLHP";

                int result = db.ExecuteNonQuery(query,
                    new SqlParameter[] {
                        new SqlParameter("@MaSV", MaSV),
                        new SqlParameter("@MaLHP", MaLHP),
                        new SqlParameter("@DiemChuyenCan", (object)diemCC ?? DBNull.Value),
                        new SqlParameter("@DiemGiuaKy", (object)diemGK ?? DBNull.Value),
                        new SqlParameter("@DiemCuoiKy", (object)diemCK ?? DBNull.Value),
                        new SqlParameter("@DiemTongKet", (object)diemTongKet ?? DBNull.Value)
                    });

                TempData[result > 0 ? "SuccessMessage" : "ErrorMessage"] =
                    result > 0 ? "Cập nhật điểm thành công!" : "Không có thay đổi.";

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                return RedirectToAction("Edit", new { MaSV, MaLHP });
            }
        }

        // ============================================
        // DELETE
        // ============================================
        public ActionResult Delete(string MaSV, string maLHP)
        {
            if (CurrentRole == "Student" && MaSV != GetMaSVFromAccount(CurrentUserId))
                return HttpNotFound();

            string query = @"
                SELECT dk.*, sv.HoTenSV, mh.TenMH, hk.TenHK
                FROM DangKyHocPhan dk
                INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            var dt = db.ExecuteQuery(query,
                new SqlParameter[] {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", maLHP)
                });

            if (dt.Rows.Count == 0) return HttpNotFound();

            DataRow row = dt.Rows[0];
            return View(new DangKyHocPhan
            {
                MaSV = row["MaSV"].ToString(),
                MaLHP = row["MaLHP"].ToString(),
                NgayDangKy = Convert.ToDateTime(row["NgayDangKy"]),
                HoTenSV = row["HoTenSV"].ToString(),
                TenMH = row["TenMH"].ToString(),
                TenHK = row["TenHK"].ToString()
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string MaSV, string maLHP)
        {
            if (CurrentRole == "Student" && MaSV != GetMaSVFromAccount(CurrentUserId))
                return HttpNotFound();

            int result = db.ExecuteNonQuery(
                "DELETE FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP",
                new SqlParameter[] {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", maLHP)
                });

            TempData[result > 0 ? "SuccessMessage" : "ErrorMessage"] =
                result > 0 ? "Hủy đăng ký thành công!" : "Không tìm thấy bản ghi.";

            return RedirectToAction("Index");
        }

        // ============================================
        // LOAD DANH SÁCH
        // ============================================
        private void LoadDanhSachSinhVien()
        {
            if (CurrentRole != "Admin") return;

            var dt = db.ExecuteQuery("SELECT MaSV, HoTenSV FROM SinhVien ORDER BY HoTenSV");
            var list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                list.Add(new SelectListItem
                {
                    Value = row["MaSV"].ToString(),
                    Text = $"{row["MaSV"]} - {row["HoTenSV"]}"
                });
            }
            ViewBag.DanhSachSinhVien = list;
        }

        private void LoadDanhSachLopHocPhan()
        {
            if (CurrentRole != "Admin") return;

            string query = @"
                SELECT lhp.MaLHP, mh.TenMH, hk.TenHK, gv.HoTenGV, lhp.SoLuongToiDa,
                       hk.NgayBatDau, hk.NgayKetThuc,
                       (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = lhp.MaLHP) AS SoSinhVienDangKy,
                       CASE WHEN GETDATE() < hk.NgayBatDau THEN '(Chưa mở) '
                            WHEN GETDATE() > hk.NgayKetThuc THEN '(Đã đóng) '
                            ELSE '(Đang mở) ' END AS TrangThaiHK
                FROM LopHocPhan lhp
                INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                ORDER BY hk.NgayBatDau DESC, mh.TenMH";

            var dt = db.ExecuteQuery(query);
            var list = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                int daDangKy = Convert.ToInt32(row["SoSinhVienDangKy"]);
                int toiDa = Convert.ToInt32(row["SoLuongToiDa"]);
                string fullText = daDangKy >= toiDa ? " [ĐÃ ĐẦY]" : "";
                list.Add(new SelectListItem
                {
                    Value = row["MaLHP"].ToString(),
                    Text = $"{row["MaLHP"]} - {row["TenMH"]} ({row["TenHK"]}) {row["TrangThaiHK"]} ({daDangKy}/{toiDa}){fullText}",
                    Disabled = daDangKy >= toiDa
                });
            }
            ViewBag.DanhSachLopHocPhan = list;
        }

        private void LoadDanhSachLopHocPhan_TheoKhoaSV(string maSV)
        {
            var dt = db.ExecuteQuery(
                @"SELECT sv.MaLop, l.MaKhoa
                  FROM SinhVien sv
                  INNER JOIN Lop l ON sv.MaLop = l.MaLop
                  WHERE sv.MaSV = @MaSV",
                new SqlParameter[] { new SqlParameter("@MaSV", maSV) }
            );

            if (dt.Rows.Count == 0)
            {
                ViewBag.DanhSachLopHocPhan = new List<SelectListItem>();
                return;
            }

            string maKhoa = dt.Rows[0]["MaKhoa"].ToString();

            string lhpQuery = @"
                SELECT lhp.MaLHP, mh.TenMH, hk.TenHK, gv.HoTenGV, lhp.SoLuongToiDa,
                       hk.NgayBatDau, hk.NgayKetThuc,
                       (SELECT COUNT(*) FROM DangKyHocPhan WHERE MaLHP = lhp.MaLHP) AS SoSinhVienDangKy,
                       CASE WHEN GETDATE() < hk.NgayBatDau THEN '(Chưa mở) '
                            WHEN GETDATE() > hk.NgayKetThuc THEN '(Đã đóng) '
                            ELSE '(Đang mở) ' END AS TrangThaiHK
                FROM LopHocPhan lhp
                INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                WHERE mh.MaKhoa = @MaKhoa
                ORDER BY hk.NgayBatDau DESC, mh.TenMH";

            var lhpDt = db.ExecuteQuery(lhpQuery,
                new SqlParameter[] { new SqlParameter("@MaKhoa", maKhoa) });

            var list = new List<SelectListItem>();
            foreach (DataRow row in lhpDt.Rows)
            {
                int daDangKy = Convert.ToInt32(row["SoSinhVienDangKy"]);
                int toiDa = Convert.ToInt32(row["SoLuongToiDa"]);
                string fullText = daDangKy >= toiDa ? " [ĐÃ ĐẦY]" : "";
                list.Add(new SelectListItem
                {
                    Value = row["MaLHP"].ToString(),
                    Text = $"{row["MaLHP"]} - {row["TenMH"]} ({row["TenHK"]}) {row["TrangThaiHK"]} ({daDangKy}/{toiDa}){fullText}",
                    Disabled = daDangKy >= toiDa
                });
            }
            ViewBag.DanhSachLopHocPhan = list;
        }
    }
}