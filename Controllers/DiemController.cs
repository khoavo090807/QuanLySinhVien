// ============================================
// CONTROLLERS/DIEMCONTROLLER.CS
// FIX LỖI: Type 'DiemController' already defines a member called 'NhapDiem'
// ============================================

using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class DiemController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

        // ========== ACTION: XEM ĐIỂM ==========
        public ActionResult XemDiem(string maSV)
        {
            LoadDanhSachSinhVien();

            List<DiemThongKe> danhSachDiem = new List<DiemThongKe>();
            ViewBag.MaSV = maSV;

            if (!string.IsNullOrEmpty(maSV))
            {
                string query = @"SELECT 
                                sv.MaSV, 
                                sv.HoTenSV, 
                                dk.MaLHP,
                                mh.TenMH,
                                mh.SoTinChi,
                                dk.DiemChuyenCan,
                                dk.DiemGiuaKy,
                                dk.DiemCuoiKy,
                                dk.DiemTongKet,
                                dk.NgayDangKy
                                FROM DangKyHocPhan dk
                                INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                                INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                                INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                                WHERE dk.MaSV = @MaSV
                                ORDER BY dk.NgayDangKy DESC";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV)
                };

                DataTable dt = db.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    float? diemTK = row["DiemTongKet"] != DBNull.Value ?
                        (float?)Convert.ToDouble(row["DiemTongKet"]) : null;

                    DiemThongKe diem = new DiemThongKe
                    {
                        MaSV = row["MaSV"].ToString(),
                        HoTenSV = row["HoTenSV"].ToString(),
                        MaLHP = row["MaLHP"].ToString(),
                        TenMH = row["TenMH"].ToString(),
                        SoTinChi = Convert.ToInt32(row["SoTinChi"]),
                        DiemChuyenCan = row["DiemChuyenCan"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemChuyenCan"]) : null,
                        DiemGiuaKy = row["DiemGiuaKy"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemGiuaKy"]) : null,
                        DiemCuoiKy = row["DiemCuoiKy"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemCuoiKy"]) : null,
                        DiemTongKet = diemTK,
                        XepLoai = XepLoaiDiem(diemTK),
                        NgayDangKy = Convert.ToDateTime(row["NgayDangKy"])
                    };
                    danhSachDiem.Add(diem);
                }

                // Tính điểm trung bình
                if (danhSachDiem.Count > 0)
                {
                    float tongDiem = 0;
                    int soMonHopLe = 0;

                    foreach (var diem in danhSachDiem)
                    {
                        if (diem.DiemTongKet.HasValue && diem.DiemTongKet >= 0)
                        {
                            tongDiem += diem.DiemTongKet.Value;
                            soMonHopLe++;
                        }
                    }

                    ViewBag.DiemTrungBinh = soMonHopLe > 0 ?
                        Math.Round((decimal)tongDiem / soMonHopLe, 2) : 0;
                    ViewBag.TongTinChi = CalculateTotalCredits(danhSachDiem);
                }
            }

            return View(danhSachDiem);
        }

        // ========== ACTION: NHẬP ĐIỂM ==========
        // GET: Hiển thị form nhập điểm
        [HttpGet]
        public ActionResult NhapDiem(string maSV = null)
        {
            LoadDanhSachSinhVien();

            ViewBag.MaSV = maSV;
            List<DiemThongKe> danhSachMonHoc = new List<DiemThongKe>();

            if (!string.IsNullOrEmpty(maSV))
            {
                string query = @"SELECT 
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

                DataTable dt = db.ExecuteQuery(query, parameters);

                foreach (DataRow row in dt.Rows)
                {
                    danhSachMonHoc.Add(new DiemThongKe
                    {
                        MaLHP = row["MaLHP"].ToString(),
                        TenMH = row["TenMH"].ToString(),
                        SoTinChi = Convert.ToInt32(row["SoTinChi"]),
                        DiemChuyenCan = row["DiemChuyenCan"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemChuyenCan"]) : null,
                        DiemGiuaKy = row["DiemGiuaKy"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemGiuaKy"]) : null,
                        DiemCuoiKy = row["DiemCuoiKy"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemCuoiKy"]) : null,
                        DiemTongKet = row["DiemTongKet"] != DBNull.Value ?
                            (float?)Convert.ToDouble(row["DiemTongKet"]) : null
                    });
                }
            }

            ViewBag.DanhSachMonHoc = danhSachMonHoc;
            return View();
        }

        // POST: Lưu điểm vào database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NhapDiem(string maSV, FormCollection form)
        {
            try
            {
                if (string.IsNullOrEmpty(maSV))
                {
                    TempData["ErrorMessage"] = "Vui lòng chọn sinh viên!";
                    return RedirectToAction("NhapDiem");
                }

                // Lấy danh sách môn học của sinh viên này
                string getMonQuery = @"SELECT MaLHP FROM DangKyHocPhan WHERE MaSV = @MaSV";
                SqlParameter[] getMonParams = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV)
                };

                DataTable dtMon = db.ExecuteQuery(getMonQuery, getMonParams);

                // Duyệt qua từng môn và lưu điểm
                foreach (DataRow mon in dtMon.Rows)
                {
                    string maLHP = mon["MaLHP"].ToString();
                    string keyChuongChan = "DiemChuyenCan_" + maLHP;
                    string keyGiuaKy = "DiemGiuaKy_" + maLHP;
                    string keyCuoiKy = "DiemCuoiKy_" + maLHP;

                    // Lấy giá trị từ form
                    float? diemCC = null, diemGK = null, diemCK = null, diemTK = null;

                    if (!string.IsNullOrEmpty(form[keyChuongChan]) && float.TryParse(form[keyChuongChan], out float cc))
                        diemCC = cc;

                    if (!string.IsNullOrEmpty(form[keyGiuaKy]) && float.TryParse(form[keyGiuaKy], out float gk))
                        diemGK = gk;

                    if (!string.IsNullOrEmpty(form[keyCuoiKy]) && float.TryParse(form[keyCuoiKy], out float ck))
                        diemCK = ck;

                    // Tính điểm tổng kết nếu có cả 3 điểm
                    if (diemCC.HasValue && diemGK.HasValue && diemCK.HasValue)
                    {
                        diemTK = (diemCC.Value * 0.1f) + (diemGK.Value * 0.3f) + (diemCK.Value * 0.6f);
                        diemTK = (float)Math.Round(diemTK.Value, 2);
                    }

                    // Cập nhật vào database
                    string updateQuery = @"UPDATE DangKyHocPhan 
                                         SET DiemChuyenCan = @DiemChuyenCan,
                                             DiemGiuaKy = @DiemGiuaKy,
                                             DiemCuoiKy = @DiemCuoiKy,
                                             DiemTongKet = @DiemTongKet
                                         WHERE MaSV = @MaSV AND MaLHP = @MaLHP";

                    SqlParameter[] updateParams = new SqlParameter[]
                    {
                        new SqlParameter("@MaSV", maSV),
                        new SqlParameter("@MaLHP", maLHP),
                        new SqlParameter("@DiemChuyenCan", (object)diemCC ?? DBNull.Value),
                        new SqlParameter("@DiemGiuaKy", (object)diemGK ?? DBNull.Value),
                        new SqlParameter("@DiemCuoiKy", (object)diemCK ?? DBNull.Value),
                        new SqlParameter("@DiemTongKet", (object)diemTK ?? DBNull.Value)
                    };

                    db.ExecuteNonQuery(updateQuery, updateParams);
                }

                TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
                return RedirectToAction("XemDiem", new { maSV = maSV });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
                LoadDanhSachSinhVien();
                return View();
            }
        }

        // ========== ACTION: BẢNG ĐIỂM LỚP ==========
        public ActionResult BangDiemLop(string maLop, string maHK)
        {
            LoadDanhSachLop();
            LoadDanhSachHocKy();

            ViewBag.MaLop = maLop;
            ViewBag.MaHK = maHK;

            List<DiemThongKe> danhSachDiem = new List<DiemThongKe>();

            if (string.IsNullOrEmpty(maLop))
            {
                return View(danhSachDiem);
            }

            string query = @"SELECT DISTINCT
                            sv.MaSV,
                            sv.HoTenSV,
                            dk.MaLHP,
                            mh.TenMH,
                            mh.SoTinChi,
                            dk.DiemChuyenCan,
                            dk.DiemGiuaKy,
                            dk.DiemCuoiKy,
                            dk.DiemTongKet,
                            hk.MaHK,
                            dk.NgayDangKy
                            FROM SinhVien sv
                            LEFT JOIN DangKyHocPhan dk ON sv.MaSV = dk.MaSV
                            LEFT JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            LEFT JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            LEFT JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            WHERE sv.MaLop = @MaLop";

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@MaLop", maLop));

            if (!string.IsNullOrEmpty(maHK))
            {
                query += " AND hk.MaHK = @MaHK";
                parameters.Add(new SqlParameter("@MaHK", maHK));
            }

            query += " ORDER BY sv.HoTenSV, mh.TenMH";

            DataTable dt = db.ExecuteQuery(query, parameters.ToArray());

            foreach (DataRow row in dt.Rows)
            {
                float? diemTK = row["DiemTongKet"] != DBNull.Value ?
                    (float?)Convert.ToDouble(row["DiemTongKet"]) : null;

                danhSachDiem.Add(new DiemThongKe
                {
                    MaSV = row["MaSV"].ToString(),
                    HoTenSV = row["HoTenSV"].ToString(),
                    MaLHP = row["MaLHP"] != DBNull.Value ? row["MaLHP"].ToString() : "",
                    TenMH = row["TenMH"] != DBNull.Value ? row["TenMH"].ToString() : "",
                    SoTinChi = row["SoTinChi"] != DBNull.Value ? Convert.ToInt32(row["SoTinChi"]) : 0,
                    DiemChuyenCan = row["DiemChuyenCan"] != DBNull.Value ?
                        (float?)Convert.ToDouble(row["DiemChuyenCan"]) : null,
                    DiemGiuaKy = row["DiemGiuaKy"] != DBNull.Value ?
                        (float?)Convert.ToDouble(row["DiemGiuaKy"]) : null,
                    DiemCuoiKy = row["DiemCuoiKy"] != DBNull.Value ?
                        (float?)Convert.ToDouble(row["DiemCuoiKy"]) : null,
                    DiemTongKet = diemTK,
                    XepLoai = XepLoaiDiem(diemTK)
                });
            }

            return View(danhSachDiem);
        }

        // ========== HELPER METHODS ==========

        private string XepLoaiDiem(float? diem)
        {
            if (!diem.HasValue) return "Chưa có";

            if (diem >= 8.5f) return "Giỏi";
            if (diem >= 7.0f) return "Khá";
            if (diem >= 5.5f) return "Trung bình";
            if (diem >= 4.0f) return "Yếu";
            return "Kém";
        }

        private int CalculateTotalCredits(List<DiemThongKe> danhSachDiem)
        {
            int total = 0;
            foreach (var diem in danhSachDiem)
            {
                if (diem.DiemTongKet.HasValue && diem.DiemTongKet >= 4.0f)
                {
                    total += diem.SoTinChi;
                }
            }
            return total;
        }

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
                    Text = row["TenLop"].ToString()
                });
            }

            ViewBag.DanhSachLop = danhSach;
        }

        private void LoadDanhSachHocKy()
        {
            string query = "SELECT MaHK, TenHK FROM HocKy ORDER BY NgayBatDau DESC";
            DataTable dt = db.ExecuteQuery(query);

            List<SelectListItem> danhSach = new List<SelectListItem>();
            danhSach.Add(new SelectListItem { Value = "", Text = "-- Tất cả học kỳ --" });

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