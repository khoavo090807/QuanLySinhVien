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

        // ✅ Helper parse float từ DB — giữ đúng phần thập phân
        private float? ParseDbFloat(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return null;
            string s = dbValue.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            // thử parse theo invariant (. hoặc ,)
            if (float.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var vInv))
                return vInv;
            // thử parse theo vi-VN
            if (float.TryParse(s, NumberStyles.Float, new CultureInfo("vi-VN"), out var vVi))
                return vVi;

            try { return Convert.ToSingle(dbValue); } catch { return null; }
        }

        // ✅ Helper parse từ input form
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

        // GET: DangKyHocPhan
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

        // GET: DangKyHocPhan/Create
        public ActionResult Create()
        {
            LoadDanhSachSinhVien();
            LoadDanhSachLopHocPhan();
            return View();
        }

        // POST: DangKyHocPhan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string MaSV, string MaLHP)
        {
            try
            {
                string checkLHPQuery = "SELECT COUNT(*) FROM LopHocPhan WHERE MaLHP = @MaLHP";
                SqlParameter[] checkLHPParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };
                int lhpCount = Convert.ToInt32(db.ExecuteScalar(checkLHPQuery, checkLHPParams));

                if (lhpCount == 0)
                {
                    TempData["ErrorMessage"] = $"Lỗi: Mã lớp học phần [{MaLHP}] không tồn tại.";
                    LoadDanhSachSinhVien();
                    LoadDanhSachLopHocPhan();
                    return View();
                }

                string checkDupQuery = "SELECT COUNT(*) FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";
                SqlParameter[] checkDupParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };
                int dupCount = Convert.ToInt32(db.ExecuteScalar(checkDupQuery, checkDupParams));

                if (dupCount > 0)
                {
                    TempData["ErrorMessage"] = $"Lỗi: Sinh viên [{MaSV}] đã đăng ký lớp [{MaLHP}] này rồi.";
                    LoadDanhSachSinhVien();
                    LoadDanhSachLopHocPhan();
                    return View();
                }

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

                string getMHQuery = "SELECT MaMH FROM LopHocPhan WHERE MaLHP = @MaLHP";
                SqlParameter[] getMHParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };
                object maMHObj = db.ExecuteScalar(getMHQuery, getMHParams);

                if (maMHObj != null)
                {
                    string maMH = maMHObj.ToString();
                    string prereqQuery = "SELECT dbo.fn_KiemTraTienQuyet(@MaSV, @MaMH)";
                    SqlParameter[] prereqParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaSV", MaSV),
                        new SqlParameter("@MaMH", maMH)
                    };
                    int prereqResult = Convert.ToInt32(db.ExecuteScalar(prereqQuery, prereqParams));

                    if (prereqResult == 0)
                    {
                        TempData["ErrorMessage"] = $"Lỗi: Sinh viên [{MaSV}] không đủ điều kiện tiên quyết để đăng ký môn này.";
                        LoadDanhSachSinhVien();
                        LoadDanhSachLopHocPhan();
                        return View();
                    }
                }

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };

                db.ExecuteStoredProcedureNonQuery("sp_DangKyMonHoc", parameters);

                string checkResultQuery = "SELECT COUNT(*) FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";
                SqlParameter[] checkResultParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };
                int registrationCount = Convert.ToInt32(db.ExecuteScalar(checkResultQuery, checkResultParams));

                if (registrationCount > 0)
                {
                    TempData["SuccessMessage"] = "Đăng ký học phần thành công!";
                    return RedirectToAction("Index", new { maSV = MaSV });
                }
                else
                {
                    TempData["ErrorMessage"] = $"Lỗi: Không thể đăng ký lớp [{MaLHP}]. Vui lòng kiểm tra lại.";
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2812)
                {
                    TempData["ErrorMessage"] = "Lỗi: Stored procedure 'sp_DangKyMonHoc' chưa được tạo.";
                }
                else if (ex.Number == 50000)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = "Lỗi SQL Server: " + ex.Message;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            LoadDanhSachSinhVien();
            LoadDanhSachLopHocPhan();
            return View();
        }

        // ✅ GET Edit — giữ nguyên, chỉ sửa parse để không mất phần thập phân
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

        // ✅ POST Edit — cho phép nhập 1.5 hoặc 1,5, tính đúng điểm tổng kết
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
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                return RedirectToAction("Edit", new { maSV = MaSV, maLHP = MaLHP });
            }
        }

        // === Giữ nguyên Delete & Helper methods ===
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
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }
            return RedirectToAction("Index");
        }

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

        private void LoadDanhSachLopHocPhan()
        {
            string query = @"SELECT lhp.MaLHP, mh.TenMH, hk.TenHK, gv.HoTenGV
                           FROM LopHocPhan lhp
                           INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                           INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                           LEFT JOIN GiangVien gv ON lhp.MaGV = gv.MaGV
                           ORDER BY hk.TenHK, mh.TenMH";

            DataTable dt = db.ExecuteQuery(query);
            List<SelectListItem> danhSach = new List<SelectListItem>();
            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaLHP"].ToString(),
                    Text = row["MaLHP"] + " - " + row["TenMH"] + " (" + row["TenHK"] + ")"
                });
            }
            ViewBag.DanhSachLopHocPhan = danhSach;
        }
    }
}
