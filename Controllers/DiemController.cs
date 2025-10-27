// ============================================
// THAY THỂ: Action NhapDiem trong DiemController
// ============================================

using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public partial class DiemController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ============================================
        // HELPER: Parse float
        // ============================================
        private float? ParseDbFloat(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value) return null;
            string s = dbValue.ToString().Trim();
            if (string.IsNullOrEmpty(s)) return null;

            if (float.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v1))
                return v1;
            if (float.TryParse(s, NumberStyles.Float, new CultureInfo("vi-VN"), out var v2))
                return v2;

            try { return Convert.ToSingle(dbValue); } catch { return null; }
        }

        private float? ParseInputFloat(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            if (float.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var v1))
                return v1;
            if (float.TryParse(s, NumberStyles.Float, new CultureInfo("vi-VN"), out var v2))
                return v2;
            return null;
        }

        // ============================================
        // GET: Diem/NhapDiem - Load tất cả môn
        // ✅ LOAD CẢ NHỮNG MÔN CÓ ĐIỂM RỒI
        // ============================================
        [HttpGet]
        public ActionResult NhapDiem(string maSV = null)
        {
            LoadDanhSachSinhVien();
            ViewBag.MaSV = maSV;

            List<DiemThongKe> danhSachMonHoc = new List<DiemThongKe>();

            if (!string.IsNullOrEmpty(maSV))
            {
                // ✅ QUAN TRỌNG: Query lấy TẤT CẢ môn đã đăng ký
                // (kể cả những môn đã có điểm)
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
                        TempData["InfoMessage"] = $"Sinh viên [{maSV}] chưa đăng ký môn học nào.";
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
        // ✅ CẬP NHẬT ĐIỂM CHO TỪNG MÔN
        // ============================================
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

                // Lấy danh sách tất cả môn của sinh viên
                string getMonQuery = @"
                    SELECT DISTINCT MaLHP 
                    FROM DangKyHocPhan 
                    WHERE MaSV = @MaSV";

                SqlParameter[] getMonParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV)
                };

                DataTable dtMon = db.ExecuteQuery(getMonQuery, getMonParams);

                if (dtMon.Rows.Count == 0)
                {
                    TempData["ErrorMessage"] = $"Sinh viên [{maSV}] chưa đăng ký môn học nào!";
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
                    string formKeyTK = "DiemTongKet_" + maLHP;

                    // Parse điểm
                    float? diemCC = ParseInputFloat(form[formKeyCC]);
                    float? diemGK = ParseInputFloat(form[formKeyGK]);
                    float? diemCK = ParseInputFloat(form[formKeyCK]);
                    float? diemTK = ParseInputFloat(form[formKeyTK]);

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
    }
}