
using QuanLySinhVien.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace QuanLySinhVien.Controllers
{
    public class DangKyHocPhanController : Controller
    {
        private DatabaseHelper db = new DatabaseHelper();

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
                    DiemChuyenCan = row["DiemChuyenCan"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemChuyenCan"]) : null,
                    DiemGiuaKy = row["DiemGiuaKy"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemGiuaKy"]) : null,
                    DiemCuoiKy = row["DiemCuoiKy"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemCuoiKy"]) : null,
                    DiemTongKet = row["DiemTongKet"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemTongKet"]) : null,
                    HoTenSV = row["HoTenSV"].ToString(),
                    TenMH = row["TenMH"].ToString(),
                    TenHK = row["TenHK"].ToString()
                };
                danhSach.Add(dk);
            }

            ViewBag.MaSV = maSV;
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
                // Pre-execution validation - kiểm tra trước khi gọi stored procedure
                // 1. Kiểm tra LHP tồn tại
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

                // 2. Kiểm tra trùng đăng ký
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

                // 3. Kiểm tra ngày đăng ký (logic từ trg_KiemTraNgayDangKy) - TRƯỚC KHI gọi procedure
                string dateCheckQuery = @"
                    SELECT
                        CASE
                            WHEN GETDATE() < hk.NgayBatDau THEN 0  -- Quá sớm
                            WHEN GETDATE() > hk.NgayKetThuc THEN 1  -- Quá muộn
                            ELSE 2  -- Trong khoảng cho phép
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

                // 4. Sử dụng fn_KiemTraTienQuyet - giữ nguyên SQL function logic
                string getMHQuery = "SELECT MaMH FROM LopHocPhan WHERE MaLHP = @MaLHP";
                SqlParameter[] getMHParams = new SqlParameter[] { new SqlParameter("@MaLHP", MaLHP) };
                string maMH = db.ExecuteScalar(getMHQuery, getMHParams)?.ToString();

                if (!string.IsNullOrEmpty(maMH))
                {
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

                // Sau khi qua tất cả validation, mới gọi stored procedure
                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", MaSV),
                    new SqlParameter("@MaLHP", MaLHP)
                };

                db.ExecuteStoredProcedureNonQuery("sp_DangKyMonHoc", parameters);

                // Post-execution validation - kiểm tra kết quả thực tế
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
                    TempData["ErrorMessage"] = "Lỗi: Stored procedure 'sp_DangKyMonHoc' chưa được tạo. Vui lòng thực thi script CauTrucXuLy_Final.sql.";
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

        // GET: DangKyHocPhan/Edit
        public ActionResult Edit(string maSV, string maLHP)
        {
            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(maLHP))
            {
                return HttpNotFound();
            }

            string query = @"SELECT 
                            dk.*, sv.HoTenSV, mh.TenMH
                            FROM DangKyHocPhan dk
                            INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                            INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSV", maSV),
                new SqlParameter("@MaLHP", maLHP)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

            DataRow row = dt.Rows[0];
            DangKyHocPhan dk = new DangKyHocPhan
            {
                MaSV = row["MaSV"].ToString(),
                MaLHP = row["MaLHP"].ToString(),
                NgayDangKy = Convert.ToDateTime(row["NgayDangKy"]),
                DiemChuyenCan = row["DiemChuyenCan"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemChuyenCan"]) : null,
                DiemGiuaKy = row["DiemGiuaKy"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemGiuaKy"]) : null,
                DiemCuoiKy = row["DiemCuoiKy"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemCuoiKy"]) : null,
                DiemTongKet = row["DiemTongKet"] != DBNull.Value ? (float?)Convert.ToDouble(row["DiemTongKet"]) : null,
                HoTenSV = row["HoTenSV"].ToString(),
                TenMH = row["TenMH"].ToString()
            };

            return View(dk);
        }

        // POST: DangKyHocPhan/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DangKyHocPhan dangKy)
        {
            try
            {
                string query = @"UPDATE DangKyHocPhan 
                               SET DiemChuyenCan = @DiemChuyenCan,
                                   DiemGiuaKy = @DiemGiuaKy,
                                   DiemCuoiKy = @DiemCuoiKy
                               WHERE MaSV = @MaSV AND MaLHP = @MaLHP";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", dangKy.MaSV),
                    new SqlParameter("@MaLHP", dangKy.MaLHP),
                    new SqlParameter("@DiemChuyenCan", (object)dangKy.DiemChuyenCan ?? DBNull.Value),
                    new SqlParameter("@DiemGiuaKy", (object)dangKy.DiemGiuaKy ?? DBNull.Value),
                    new SqlParameter("@DiemCuoiKy", (object)dangKy.DiemCuoiKy ?? DBNull.Value)
                };

                int result = db.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Cập nhật điểm thành công!";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return View(dangKy);
        }

        // GET: DangKyHocPhan/Delete
        public ActionResult Delete(string maSV, string maLHP)
        {
            if (string.IsNullOrEmpty(maSV) || string.IsNullOrEmpty(maLHP))
            {
                return HttpNotFound();
            }

            string query = @"SELECT 
                            dk.*, sv.HoTenSV, mh.TenMH, hk.TenHK
                            FROM DangKyHocPhan dk
                            INNER JOIN SinhVien sv ON dk.MaSV = sv.MaSV
                            INNER JOIN LopHocPhan lhp ON dk.MaLHP = lhp.MaLHP
                            INNER JOIN MonHoc mh ON lhp.MaMH = mh.MaMH
                            INNER JOIN HocKy hk ON lhp.MaHK = hk.MaHK
                            WHERE dk.MaSV = @MaSV AND dk.MaLHP = @MaLHP";

            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@MaSV", maSV),
                new SqlParameter("@MaLHP", maLHP)
            };

            DataTable dt = db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
            {
                return HttpNotFound();
            }

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

        // POST: DangKyHocPhan/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string maSV, string maLHP)
        {
            try
            {
                string query = "DELETE FROM DangKyHocPhan WHERE MaSV = @MaSV AND MaLHP = @MaLHP";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@MaSV", maSV),
                    new SqlParameter("@MaLHP", maLHP)
                };

                int result = db.ExecuteNonQuery(query, parameters);

                if (result > 0)
                {
                    TempData["SuccessMessage"] = "Hủy đăng ký học phần thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        // Helper methods
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