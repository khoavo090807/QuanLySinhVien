using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class DiemController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();
        
        
     
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
        // GET: Diem/NhapDiem - Load CHỈ những môn CHƯA có điểm tổng kết
        // ============================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public ActionResult NhapDiem(string maSV = null)
        {
            
            LoadDanhSachSinhVien();
            ViewBag.MaSV = maSV;

            List<DiemThongKe> danhSachMonHoc = new List<DiemThongKe>();

            if (!string.IsNullOrEmpty(maSV))
            {
                // ✅ QUAN TRỌNG: Chỉ load những môn CHƯA có DiemTongKet (DiemTongKet IS NULL)
                string query = @"
                    SELECT 
                        dk.MaLHP,
                        mh.TenMH,
                        mh.SoTinChi,
                        dk.DiemChuyenCan,
                        dk.DiemGiuaKy,
                        dk.DiemCuoiKy,
                        dk.DiemTongKet
                    FROM DangKyHocPhan dk
                    INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                    INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                    WHERE dk.MaSV = @MaSV
                      AND dk.DiemTongKet IS NULL
                    ORDER BY mh.TenMH";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV)
                };

                try
                {
                    DataTable dt = db.ExecuteQuery(query, parameters);

                    if (dt.Rows.Count > 0)
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            danhSachMonHoc.Add(new DiemThongKe
                            {
                                MaLHP = row["MaLHP"].ToString(),
                                TenMH = row["TenMH"].ToString(),
                                SoTinChi = row["SoTinChi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SoTinChi"]),

                                DiemChuyenCan = ParseDbFloat(row["DiemChuyenCan"]),
                                DiemGiuaKy = ParseDbFloat(row["DiemGiuaKy"]),
                                DiemCuoiKy = ParseDbFloat(row["DiemCuoiKy"]),
                                DiemTongKet = ParseDbFloat(row["DiemTongKet"])
                            });
                        }
                    }
                    else
                    {
                        TempData["InfoMessage"] = $"Sinh viên [{maSV}] không có môn nào cần nhập điểm (hoặc tất cả đã có điểm).";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi tải danh sách: {ex.Message}";
                }
            }

            ViewBag.DanhSachMonHoc = danhSachMonHoc;
            return View();
        }

        // ============================================
        // POST: Diem/NhapDiem - Lưu tất cả điểm
        // ============================================
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NhapDiem(string maSV, FormCollection form)
        {
            
            try
            {
                if (string.IsNullOrEmpty(maSV))
                {
                    TempData["ErrorMessage"] = "Lỗi: Vui lòng chọn sinh viên!";
                    return RedirectToAction("NhapDiem");
                }

                // Lấy danh sách tất cả môn của sinh viên CHƯA có điểm
                string getMonQuery = @"
                    SELECT DISTINCT dk.MaLHP 
                    FROM DangKyHocPhan dk
                    WHERE dk.MaSV = @MaSV
                      AND dk.DiemTongKet IS NULL";

                SqlParameter[] getMonParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV)
                };

                DataTable dtMon = db.ExecuteQuery(getMonQuery, getMonParams);

                if (dtMon.Rows.Count == 0)
                {
                    TempData["ErrorMessage"] = $"Sinh viên [{maSV}] không có môn nào cần nhập điểm!";
                    return RedirectToAction("NhapDiem");
                }

                int soMonLuu = 0;
                string loi = "";

                // Duyệt từng môn
                foreach (DataRow mon in dtMon.Rows)
                {
                    string maLHP = mon["MaLHP"].ToString();

                    // Lấy tên input từ form
                    string formKeyCC = "DiemChuyenCan_" + maLHP;
                    string formKeyGK = "DiemGiuaKy_" + maLHP;
                    string formKeyCK = "DiemCuoiKy_" + maLHP;

                    // Parse điểm
                    float? diemCC = ParseInputFloat(form[formKeyCC]);
                    float? diemGK = ParseInputFloat(form[formKeyGK]);
                    float? diemCK = ParseInputFloat(form[formKeyCK]);

                    // Tính điểm tổng kết
                    float? diemTK = null;
                    if (diemCC.HasValue && diemGK.HasValue && diemCK.HasValue)
                    {
                        diemTK = (float)Math.Round((diemCC.Value * 0.1f) + (diemGK.Value * 0.3f) + (diemCK.Value * 0.6f), 2);
                    }

                    // Nếu không nhập gì → skip môn này
                    if (!diemCC.HasValue && !diemGK.HasValue && !diemCK.HasValue)
                    {
                        continue;
                    }

                    // Cập nhật database
                    try
                    {
                        string updateQuery = @"
                            UPDATE DangKyHocPhan 
                            SET DiemChuyenCan = @DiemCC,
                                DiemGiuaKy = @DiemGK,
                                DiemCuoiKy = @DiemCK,
                                DiemTongKet = @DiemTK
                            WHERE MaSV = @MaSV AND MaLHP = @MaLHP";

                        SqlParameter[] updateParams = new SqlParameter[]
                        {
                            new SqlParameter("@MaSV", maSV),
                            new SqlParameter("@MaLHP", maLHP),
                            new SqlParameter("@DiemCC", (object)diemCC ?? DBNull.Value),
                            new SqlParameter("@DiemGK", (object)diemGK ?? DBNull.Value),
                            new SqlParameter("@DiemCK", (object)diemCK ?? DBNull.Value),
                            new SqlParameter("@DiemTK", (object)diemTK ?? DBNull.Value)
                        };

                        int result = db.ExecuteNonQuery(updateQuery, updateParams);
                        if (result > 0)
                        {
                            soMonLuu++;
                        }
                    }
                    catch (Exception exUpdate)
                    {
                        loi += $"• Môn [{maLHP}]: {exUpdate.Message}\n";
                    }
                }

                // Hiển thị kết quả
                if (soMonLuu > 0)
                {
                    TempData["SuccessMessage"] = $"✅ Lưu thành công {soMonLuu} môn cho sinh viên [{maSV}]";
                    if (!string.IsNullOrEmpty(loi))
                    {
                        TempData["ErrorMessage"] = $"⚠️ Lỗi:\n{loi}";
                    }
                }
                else
                {
                    TempData["InfoMessage"] = "Không có dữ liệu mới để lưu (bạn chưa nhập điểm)";
                }

                return RedirectToAction("NhapDiem", new { maSV = maSV });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"❌ Lỗi: {ex.Message}";
                return RedirectToAction("NhapDiem", new { maSV = maSV });
            }
        }

        // ============================================
        // GET: Diem/XemDiem - Xem điểm sinh viên
        // ============================================
        [HttpGet]
        public ActionResult XemDiem(string maSV = null)
        {
            List<DiemThongKe> danhSach = new List<DiemThongKe>();

            if (!string.IsNullOrEmpty(maSV))
            {
                string query = @"
                    SELECT 
                        dk.MaSV,
                        sv.HoTenSV,
                        dk.MaLHP,
                        mh.TenMH,
                        mh.SoTinChi,
                        dk.DiemChuyenCan,
                        dk.DiemGiuaKy,
                        dk.DiemCuoiKy,
                        dk.DiemTongKet,
                        CASE 
                            WHEN dk.DiemTongKet IS NULL THEN 'Chưa có'
                            WHEN dk.DiemTongKet >= 8.5 THEN 'Giỏi'
                            WHEN dk.DiemTongKet >= 7.0 THEN 'Khá'
                            WHEN dk.DiemTongKet >= 5.5 THEN 'Trung bình'
                            WHEN dk.DiemTongKet >= 4.0 THEN 'Yếu'
                            ELSE 'Kém'
                        END AS XepLoai
                    FROM DangKyHocPhan dk
                    INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                    INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                    INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                    WHERE dk.MaSV = @MaSV
                    ORDER BY mh.TenMH";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV)
                };

                try
                {
                    DataTable dt = db.ExecuteQuery(query, parameters);

                    if (dt.Rows.Count > 0)
                    {
                        ViewBag.TenSV = dt.Rows[0]["HoTenSV"].ToString();
                        ViewBag.MaSVHienThi = maSV;

                        // Tính điểm trung bình
                        float tongDiem = 0;
                        int soMonCoTK = 0;
                        int tongTinChi = 0;

                        foreach (DataRow row in dt.Rows)
                        {
                            float? diemTK = ParseDbFloat(row["DiemTongKet"]);
                            int tinChi = row["SoTinChi"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoTinChi"]);

                            if (diemTK.HasValue)
                            {
                                tongDiem += diemTK.Value;
                                soMonCoTK++;
                                tongTinChi += tinChi;
                            }

                            danhSach.Add(new DiemThongKe
                            {
                                MaSV = row["MaSV"].ToString(),
                                HoTenSV = row["HoTenSV"].ToString(),
                                MaLHP = row["MaLHP"].ToString(),
                                TenMH = row["TenMH"].ToString(),
                                SoTinChi = tinChi,
                                DiemChuyenCan = ParseDbFloat(row["DiemChuyenCan"]),
                                DiemGiuaKy = ParseDbFloat(row["DiemGiuaKy"]),
                                DiemCuoiKy = ParseDbFloat(row["DiemCuoiKy"]),
                                DiemTongKet = diemTK,
                                XepLoai = row["XepLoai"].ToString()
                            });
                        }

                        // Tính trung bình
                        ViewBag.DiemTrungBinh = soMonCoTK > 0 ? (tongDiem / soMonCoTK).ToString("0.00") : "Chưa có";
                        ViewBag.TongTinChi = tongTinChi;
                    }
                    else
                    {
                        TempData["InfoMessage"] = $"Sinh viên [{maSV}] chưa đăng ký môn học nào.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                }
            }

            LoadDanhSachSinhVien();
            ViewBag.MaSV = maSV;
            return View(danhSach);
        }

        // ============================================
        // GET: Diem/BangDiemLop - Xem bảng điểm lớp
        // ============================================
        [HttpGet]
        public ActionResult BangDiemLop(string maLop = null, string maHK = null)
        {
            List<DiemThongKe> danhSach = new List<DiemThongKe>();

            if (!string.IsNullOrEmpty(maLop))
            {
                string query = @"
                    SELECT 
                        dk.MaSV,
                        sv.HoTenSV,
                        dk.MaLHP,
                        mh.TenMH,
                        mh.SoTinChi,
                        dk.DiemChuyenCan,
                        dk.DiemGiuaKy,
                        dk.DiemCuoiKy,
                        dk.DiemTongKet,
                        CASE 
                            WHEN dk.DiemTongKet IS NULL THEN 'Chưa có'
                            WHEN dk.DiemTongKet >= 8.5 THEN 'Giỏi'
                            WHEN dk.DiemTongKet >= 7.0 THEN 'Khá'
                            WHEN dk.DiemTongKet >= 5.5 THEN 'Trung bình'
                            WHEN dk.DiemTongKet >= 4.0 THEN 'Yếu'
                            ELSE 'Kém'
                        END AS XepLoai
                    FROM DangKyHocPhan dk
                    INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                    INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                    INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                    INNER JOIN Lop l ON sv.MaLop = l.MaLop
                    WHERE l.MaLop = @MaLop";

                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("@MaLop", maLop));

                if (!string.IsNullOrEmpty(maHK))
                {
                    query += " AND lhp.MaHK = @MaHK";
                    parameters.Add(new SqlParameter("@MaHK", maHK));
                }

                query += " ORDER BY sv.MaSV, mh.TenMH";

                try
                {
                    DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

                    foreach (DataRow row in dt.Rows)
                    {
                        danhSach.Add(new DiemThongKe
                        {
                            MaSV = row["MaSV"].ToString(),
                            HoTenSV = row["HoTenSV"].ToString(),
                            MaLHP = row["MaLHP"].ToString(),
                            TenMH = row["TenMH"].ToString(),
                            SoTinChi = row["SoTinChi"] == DBNull.Value ? (int?)null : Convert.ToInt32(row["SoTinChi"]),
                            DiemChuyenCan = ParseDbFloat(row["DiemChuyenCan"]),
                            DiemGiuaKy = ParseDbFloat(row["DiemGiuaKy"]),
                            DiemCuoiKy = ParseDbFloat(row["DiemCuoiKy"]),
                            DiemTongKet = ParseDbFloat(row["DiemTongKet"]),
                            XepLoai = row["XepLoai"].ToString()
                        });
                    }

                    if (danhSach.Count == 0)
                    {
                        TempData["InfoMessage"] = "Không có dữ liệu điểm cho lớp này.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
                }
            }

            LoadDanhSachLop();
            LoadDanhSachHocKy();
            ViewBag.MaLop = maLop;
            ViewBag.MaHK = maHK;
            return View(danhSach);
        }

        // ============================================
        // HELPER: Load danh sách sinh viên
        // ============================================
        private void LoadDanhSachSinhVien()
        {
            string query = "SELECT MaSV, HoTenSV FROM SinhVien ORDER BY HoTenSV";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            danhSach.Add(new SelectListItem { Value = "", Text = "-- Chọn sinh viên --" });

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
        // HELPER: Load danh sách lớp
        // ============================================
        private void LoadDanhSachLop()
        {
            string query = "SELECT MaLop, TenLop FROM Lop ORDER BY TenLop";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            danhSach.Add(new SelectListItem { Value = "", Text = "-- Chọn lớp --" });

            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaLop"].ToString(),
                    Text = row["MaLop"] + " - " + row["TenLop"].ToString()
                });
            }

            ViewBag.DanhSachLop = danhSach;
        }

        // ============================================
        // HELPER: Load danh sách học kỳ
        // ============================================
        private void LoadDanhSachHocKy()
        {
            string query = "SELECT MaHK, TenHK FROM HocKy ORDER BY NgayBatDau DESC";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            danhSach.Add(new SelectListItem { Value = "", Text = "-- Chọn học kỳ --" });

            foreach (DataRow row in dt.Rows)
            {
                danhSach.Add(new SelectListItem
                {
                    Value = row["MaHK"].ToString(),
                    Text = row["TenHK"].ToString()
                });
            }

            ViewBag.DanhSachHocKy = danhSach;
        }
    }
}